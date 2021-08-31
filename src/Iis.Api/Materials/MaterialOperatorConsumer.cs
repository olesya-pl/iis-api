﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IIS.Core.Materials;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Iis.Services.Contracts.Interfaces;
using Iis.DbLayer.MaterialDictionaries;
using IIS.Services.Contracts.Interfaces;
using Iis.Services.Contracts.Params;
using System.Collections.Generic;
using Iis.DataModel.Materials;
using Iis.Services.Contracts.Materials.Distribution;
using System.Diagnostics;
using System.Text;

namespace Iis.Api.Materials
{
    public class MaterialOperatorConsumer : BackgroundService
    {
        private const int MaterialsPage = 1;
        private const int Timeout = 30;

        private readonly IUserService _userService;
        private readonly ILogger<MaterialOperatorConsumer> _logger;
        private readonly IMaterialService _materialService;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMaterialDistributionService _materialDistributionService;
        private Dictionary<string, Guid> _channelRoleMapping;

        private readonly IReadOnlyList<MaterialDistributionRule> Rules;
        public MaterialOperatorConsumer(
            ILogger<MaterialOperatorConsumer> logger,
            IMaterialService materialService,
            IUserService userService,
            IMaterialProvider materialProvider,
            IMaterialDistributionService materialDistributionService)
        {
            _userService = userService;
            _logger = logger;
            _materialService = materialService;
            _materialProvider = materialProvider;
            _materialDistributionService = materialDistributionService;

            Rules = new List<MaterialDistributionRule>
            {
                new MaterialDistributionRule
                {
                    GetMaterials = null,
                    GetRole = m => GetRoleByChannel(m.Channel),
                    Priority = 2,
                    GetChannels = () => _materialProvider.GetCellSatChannelsAsync(),
                    GetMaterialsByChannel = (limit, channel) => _materialProvider.GetCellSatWithChannelAsync(limit, channel)
                },
                new MaterialDistributionRule
                {
                    GetMaterials = limit => _materialProvider.GetCellSatWithoutChannelAsync(limit),
                    GetRole = m => GetRoleByChannel(m.Channel),
                    Priority = 1
                },
                new MaterialDistributionRule
                {
                    GetMaterials = limit => _materialProvider.GetNotCellSatAsync(limit),
                    GetRole = m => null,
                    Priority = 10
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channelRoleMapping == null)
            {
                var mappings = await _materialProvider.GetChannelMappingsAsync();
                _channelRoleMapping = mappings.ToDictionary(m => m.ChannelName, m => m.RoleId);
            }
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Distribute();
                }
                catch (Exception e)
                {
                    _logger.LogError("MaterialOperatorConsumer. Exception={e}", e);
                }
            }
        }

        private string GetRoleByChannel(string channel)
        {
            if (string.IsNullOrEmpty(channel)) return null;
            return _channelRoleMapping.ContainsKey(channel) ? 
                _channelRoleMapping.GetValueOrDefault(channel).ToString("N") :
                null;
        }

        private async Task<IEnumerable<MaterialDistributionItem>> GetMaterialsByRule(int limit, MaterialDistributionRule rule)
        {
            if (rule.GetMaterials != null)
                return (await rule.GetMaterials(limit))
                    .Select(_ => new MaterialDistributionItem(_.Id, rule.Priority, GetRoleByChannel(_.Channel), _.Channel));

            var channels = await rule.GetChannels();
            var list = new List<MaterialDistributionItem>();
            foreach (var channel in channels)
            {
                var materials = (await rule.GetMaterialsByChannel(limit, channel))
                    .Select(_ => new MaterialDistributionItem(_.Id, rule.Priority, GetRoleByChannel(_.Channel), channel));
                list.AddRange(materials);
            }
            return list;
        }

        private async Task<MaterialDistributionList> GetMaterials(int limit)
        {
            var list = new List<MaterialDistributionItem>();
            foreach (var rule in Rules)
            {
                var materials = await GetMaterialsByRule(limit, rule);
                list.AddRange(materials);
            }
            return new MaterialDistributionList(list);
        }

        private async Task Distribute()
        {
            var sw = new Stopwatch();
            sw.Start();

            var users = await _userService.GetOperatorsForMaterialsAsync();
            var totalLimit = users.TotalFreeSlots();

            var materials = await GetMaterials(totalLimit);

            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var result = _materialDistributionService.Distribute(materials, users, options);

            foreach (var item in result.Items)
            {
                await _materialService.AssignMaterialOperatorAsync(item.MaterialId, item.UserId);
            }

            var freeSlotsRemains = users.TotalFreeSlots();

            sw.Stop();
            _logger.LogInformation(GetLogMessage(result, freeSlotsRemains, sw.Elapsed));

            if (freeSlotsRemains == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(Timeout));
                return;
            }
        }

        private string GetLogMessage(DistributionResult distributionResult, int freeSlotsRemains, TimeSpan timeElapsed)
        {
            var sb = new StringBuilder();

            foreach (var item in distributionResult.Items)
            {
                sb.AppendLine($"{item.MaterialId} => {item.UserId}");
            }
            sb.AppendLine($"Free Slots Remains: {freeSlotsRemains}");
            sb.AppendLine($"Time Elapsed: {(int)timeElapsed.TotalMilliseconds} ms");

            return sb.ToString();
        }
    }
}