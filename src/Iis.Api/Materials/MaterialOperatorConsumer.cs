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

        private readonly IReadOnlyList<MaterialDistributionRule> Rules = new List<MaterialDistributionRule>
        {
            //new MaterialDistributionRule
            //{
            //    Filter = m => (m.Source.StartsWith("sat.") || m.Source.StartsWith("cell.")) && m.Channel != null
            //}
        };
            


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
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    
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

        private async Task<MaterialDistributionList> GetMaterials()
        {
            return new MaterialDistributionList();
        }

        private async Task Distribute()
        {
            var materials = await GetMaterials();
            var users = await _userService.GetOperatorsForMaterialsAsync();
            var options = new MaterialDistributionOptions { Strategy = DistributionStrategy.InSuccession };
            _materialDistributionService.Distribute(materials, users, options);
        }
    }
}