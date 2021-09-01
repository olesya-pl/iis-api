using System;
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
        private readonly IReadOnlyList<MaterialDistributionRule> _rules;

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

            _rules = new List<MaterialDistributionRule>
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
                    Priority = 0
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
            return !string.IsNullOrEmpty(channel) && _channelRoleMapping.ContainsKey(channel) ? 
                _channelRoleMapping.GetValueOrDefault(channel).ToString("N") :
                null;
        }

        private async Task<IEnumerable<MaterialDistributionItem>> GetMaterialsByRuleAsync(int limit, MaterialDistributionRule rule)
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

        private async Task<MaterialDistributionList> GetMaterialsAsync(int limit)
        {
            var list = new List<MaterialDistributionItem>();
            foreach (var rule in _rules)
            {
                var materials = await GetMaterialsByRuleAsync(limit, rule);
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

            var materials = await GetMaterialsAsync(totalLimit);

            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.Evenly };
            var distributionResult = _materialDistributionService.Distribute(materials, users, options);

            await _materialService.SaveDistributionResult(distributionResult);

            var freeSlotsRemains = users.TotalFreeSlots();

            sw.Stop();
            _logger.LogInformation(GetLogMessage(distributionResult, freeSlotsRemains, sw.Elapsed));

            if (freeSlotsRemains == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(Timeout));
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

        public int GetPriority(UserDistributionItem user, string roleName)
        {
            if (user.RoleNames.Count == 0) return 0;

            if (string.IsNullOrEmpty(roleName))
                return user.RoleNames.Count == 0 ? 0 : 1;

            if (!user.RoleNames.Contains(roleName)) return -1;
            if (user.RoleNames.Count == 1) return 2;
            return 1;
        }
    }
}