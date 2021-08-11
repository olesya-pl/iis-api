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

        public MaterialOperatorConsumer(
            ILogger<MaterialOperatorConsumer> logger,
            IMaterialService materialService,
            IUserService userService,
            IMaterialProvider materialProvider)
        {
            _userService = userService;
            _logger = logger;
            _materialService = materialService;
            _materialProvider = materialProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var availableOperators = await _userService.GetAvailableOperatorIdsAsync();
                    if (!availableOperators.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Timeout), stoppingToken);
                        continue;
                    }

                    var paginationParams = new PaginationParams(MaterialsPage, availableOperators.Count);
                    var sortingParams = new SortingParams(MaterialSortingFields.CreatedDate, SortDirections.DESC);
                    var unassignedMaterials = await _materialProvider.GetAllUnassignedIdsAsync(paginationParams, sortingParams, stoppingToken);
                    if (!unassignedMaterials.Any())
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Timeout), stoppingToken);
                        continue;
                    }

                    foreach (var (assignee, materialId) in availableOperators.Zip(unassignedMaterials))
                        await _materialService.AssignMaterialOperatorAsync(materialId, assignee);
                }
                catch (Exception e)
                {
                    _logger.LogError("MaterialOperatorAssigner. Exception={e}", e);
                }
            }
        }
    }
}