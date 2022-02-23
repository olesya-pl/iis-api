using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
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
        private readonly IChannelCoefficientEvaluator _channelCoefficientEvaluator;
        private readonly IVariableCoefficientService _coefficientService;
        private readonly IMapper _mapper;

        public DistributionDataMediator(
            IMaterialDistributionService materialDistributionService,
            IUserElasticRepository userElasticRepository,
            ISecurityLevelElasticRepository securityLevelElasticRepository,
            IDistributionData distributionData,
            ISecurityLevelChecker securityLevelChecker,
            IChannelCoefficientEvaluator channelCoefficientEvaluator,
            IVariableCoefficientService coefficientService,
            IMapper mapper)
        {
            _materialDistributionService = materialDistributionService;
            _userElasticRepository = userElasticRepository;
            _securityLevelElasticRepository = securityLevelElasticRepository;
            _distributionData = distributionData;
            _securityLevelChecker = securityLevelChecker;
            _channelCoefficientEvaluator = channelCoefficientEvaluator;
            _coefficientService = coefficientService;
            _mapper = mapper;
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
            var userEntities = await _userElasticRepository.GetOperatorsAsync(cancellationToken);

            var users = _mapper.Map<IReadOnlyList<UserDistributionInfo>>(userEntities);

            var securityLevelsPlain = await _securityLevelElasticRepository.GetSecurityLevelsPlainAsync(cancellationToken);

            _securityLevelChecker.Reload(securityLevelsPlain);

            _channelCoefficientEvaluator.ReloadUserDistributionInfos(users);

            _distributionData.Distribute(users);
        }
    }
}
