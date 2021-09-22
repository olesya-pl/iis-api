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
    public class MaterialOperatorDistributor : BackgroundService
    {
        private const int MaterialsPage = 1;
        private const int Timeout = 30;
        private const string SAT_PREFFIX = "sat.";
        private const string CELL_PREFFIX = "cell.";

        private readonly IUserService _userService;
        private readonly ILogger<MaterialOperatorDistributor> _logger;
        private readonly IMaterialService _materialService;
        private readonly IMaterialElasticService _materialElasticService;
        private readonly IMaterialProvider _materialProvider;
        private readonly IMaterialDistributionService _materialDistributionService;
        private readonly IReadOnlyList<MaterialDistributionRule> _rules;

        public MaterialOperatorDistributor(
            ILogger<MaterialOperatorDistributor> logger,
            IMaterialService materialService,
            IMaterialElasticService materialElasticService,
            IUserService userService,
            IMaterialProvider materialProvider,
            IMaterialDistributionService materialDistributionService)
        {
            _userService = userService;
            _logger = logger;
            _materialService = materialService;
            _materialElasticService = materialElasticService;
            _materialProvider = materialProvider;
            _materialDistributionService = materialDistributionService;

            _rules = new List<MaterialDistributionRule>
            {
                new MaterialDistributionRule
                {
                    Priority = 2,
                    Filter =  m => (m.Source.StartsWith(SAT_PREFFIX) || m.Source.StartsWith(CELL_PREFFIX))
                        && m.Channel != null,
                    GetChannel = m => m.Channel
                },
                new MaterialDistributionRule
                {
                    Priority = 1,
                    Filter =  m => (m.Source.StartsWith(SAT_PREFFIX) || m.Source.StartsWith(CELL_PREFFIX))
                        && m.Channel == null,
                    GetChannel = m => m.Channel
                    
                },
                new MaterialDistributionRule
                {
                    Priority = 0,
                    Filter =  m => !(m.Source.StartsWith(SAT_PREFFIX) || m.Source.StartsWith(CELL_PREFFIX)),
                    GetChannel = m => null
                }
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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

        private async Task<DistributionResult> DistributeForUser(UserDistributionItem user, IReadOnlyList<Guid> distributedIds)
        {
            var list = new List<DistributionResultItem>();

            foreach (var rule in _rules.OrderByDescending(_ => _.Priority))
            {
                var materials = await _materialProvider.GetMaterialsForDistribution(user, rule.Filter);
                list.AddRange(materials.Select(_ => new DistributionResultItem(_.Id, user.Id, rule.GetChannel(_))));
                user.FreeSlots -= materials.Count();
                if (user.FreeSlots == 0) break;
            }

            return new DistributionResult(list);
        }

        private async Task Distribute()
        {
            var sw = new Stopwatch();
            sw.Start();

            var users = await _userService.GetOperatorsForMaterialsAsync();
            foreach (var user in users.Items)
            {
                var oneUserResult = await DistributeForUser(user, new List<Guid> { });
                await _materialService.SaveDistributionResult(oneUserResult);

                var materialIds = oneUserResult.Items.Select(_ => _.MaterialId).ToList();
                await _materialElasticService.PutMaterialsToElasticSearchAsync(materialIds);

                _logger.LogInformation(GetLogMessage(oneUserResult, user, sw.Elapsed));
            }

            var freeSlotsRemains = users.TotalFreeSlots();
            sw.Stop();

            await Task.Delay(TimeSpan.FromSeconds(Timeout));
        }

        private string GetLogMessage(DistributionResult distributionResult, UserDistributionItem user, TimeSpan timeElapsed)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Materials assigned for user { user.Id }:");

            foreach (var item in distributionResult.Items)
            {
                sb.AppendLine($"{item.MaterialId} ({item.Channel})");
            }
            sb.AppendLine($"Free Slots Remains: {user.FreeSlots}");
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