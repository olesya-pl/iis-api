using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Iis.Interfaces.SecurityLevels;
using Iis.MaterialDistributor.Contracts.DataStorage;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.DataStorage
{
    public class DistributionDataMediator : IDistributionDataMediator
    {
        private readonly IMaterialDistributionService _materialDistributionService;
        private readonly IUserElasticRepository _userElasticRepository;
        private readonly ISecurityLevelElasticRepository _securityLevelElasticRepository;
        private readonly IDistributionData _distributionData;
        private readonly ISecurityLevelChecker _securityLevelChecker;
        private readonly IVariableCoefficientService _coefficientService;

        public DistributionDataMediator(
            IMaterialDistributionService materialDistributionService,
            IUserElasticRepository userElasticRepository,
            ISecurityLevelElasticRepository securityLevelElasticRepository,
            IDistributionData distributionData,
            ISecurityLevelChecker securityLevelChecker,
            IVariableCoefficientService coefficientService)
        {
            _materialDistributionService = materialDistributionService;
            _userElasticRepository = userElasticRepository;
            _securityLevelElasticRepository = securityLevelElasticRepository;
            _distributionData = distributionData;
            _securityLevelChecker = securityLevelChecker;
            _coefficientService = coefficientService;
        }

        public async Task RefreshMaterialsAsync(CancellationToken cancellationToken)
        {
            var coefficient = await _coefficientService.GetWithMaxOffsetHoursAsync(cancellationToken);

            var materialCollection = await _materialDistributionService.GetMaterialCollectionAsync(coefficient.OffsetHours, cancellationToken);

            materialCollection = await _coefficientService.SetVariableCoefficientForMaterialsAsync(DateTime.UtcNow, materialCollection, cancellationToken);

            var materials = materialCollection
                                .ToDictionary(_ => _.Id);

            _distributionData.RefreshMaterials(materials);
        }

        public async Task DistributeAsync(CancellationToken cancellationToken)
        {
            var users = await _userElasticRepository.GetOperatorsAsync(cancellationToken);

            var securityLevelsPlain = await _securityLevelElasticRepository.GetSecurityLevelsPlainAsync(cancellationToken);

            _securityLevelChecker.Reload(securityLevelsPlain);

            _distributionData.Distribute(users);
        }
    }
}
