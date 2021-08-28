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
                    Getter = limit => _materialProvider.GetCellSatWithChannel(limit),
                    Priority = 2
                },
                new MaterialDistributionRule
                {
                    Getter = limit => _materialProvider.GetCellSatWithoutChannel(limit),
                    Priority = 1
                },
                new MaterialDistributionRule
                {
                    Getter = limit => _materialProvider.GetNotCellSat(limit),
                    Priority = 10
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
                    _logger.LogError("MaterialOperatorAssigner. Exception={e}", e);
                }
            }
        }

        private async Task OldCode(CancellationToken stoppingToken)
        {
            var availableOperators = await _userService.GetAvailableOperatorIdsAsync();
            if (!availableOperators.Any())
            {
                await Task.Delay(TimeSpan.FromSeconds(Timeout), stoppingToken);
                return;
            }

            var paginationParams = new PaginationParams(MaterialsPage, availableOperators.Count);
            var sortingParams = new SortingParams(MaterialSortingFields.RegistrationDate, SortDirections.DESC);
            var unassignedMaterials = await _materialProvider.GetAllUnassignedIdsAsync(paginationParams, sortingParams, stoppingToken);
            if (!unassignedMaterials.Any())
            {
                await Task.Delay(TimeSpan.FromSeconds(Timeout), stoppingToken);
                return;
            }

            foreach (var (assignee, materialId) in availableOperators.Zip(unassignedMaterials))
                await _materialService.AssignMaterialOperatorAsync(materialId, assignee);
        }

        private async Task<MaterialDistributionList> GetMaterials(int limit)
        {
            var list = new List<MaterialDistributionDto>();
            foreach (var rule in Rules)
            {
                var materials = (await rule.Getter(limit))
                    .Select(m => new MaterialDistributionDto(m.Id, rule.Priority, null)); //TODO
                list.AddRange(materials);
            }
            return new MaterialDistributionList(list);
        }

        private async Task Distribute()
        {
            var users = await _userService.GetOperatorsForMaterialsAsync();
            var totalLimit = users.Items.Sum(u => u.FreeSlots);

            var materials = await GetMaterials(totalLimit);

            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.InSuccession };
            _materialDistributionService.Distribute(materials, users, options);
        }
    }
}