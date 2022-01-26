using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using AutoMapper;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.MaterialDistributor.Contracts.Repositories;

namespace Iis.MaterialDistributor.Services
{
    internal class VariableCoefficientService : IVariableCoefficientService
    {
        private readonly IVariableCoefficientRepository _coefficientRepository;
        private readonly IVariableCoefficientRuleEvaluator _ruleEvaluator;
        private readonly IMapper _mapper;

        public VariableCoefficientService(
            IVariableCoefficientRepository coefficientRepository,
            IVariableCoefficientRuleEvaluator ruleEvaluator,
            IMapper mapper)
        {
            _coefficientRepository = coefficientRepository;
            _ruleEvaluator = ruleEvaluator;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<MaterialDocument>> SetForMaterialsAsync(
            DateTime comparisonTimeStamp,
            IReadOnlyCollection<MaterialDocument> documentCollection,
            CancellationToken cancellationToken)
        {
            var coefficients = await _coefficientRepository.GetAllAsync(cancellationToken);

            var orderedCoefficients = coefficients
                                .Select(_ => _mapper.Map<VariableCoefficient>(_))
                                .ToArray();

            var ruleCollection = CreateVariableCoefficientRuleCollection(orderedCoefficients);

            foreach (var document in documentCollection)
            {
                document.VariableCoefficient = _ruleEvaluator.GetVariableCoefficientValue(ruleCollection, comparisonTimeStamp, document);
            }

            return documentCollection;
        }

        public async Task<VariableCoefficient> GetWithMaxOffsetHoursAsync(CancellationToken cancellationToken)
        {
            var coefficients = await _coefficientRepository.GetAllAsync(cancellationToken);

            var entity = coefficients
                            .OrderByDescending(_ => _.OffsetHours)
                            .FirstOrDefault();

            return _mapper.Map<VariableCoefficient>(entity);
        }

        private static IReadOnlyCollection<VariableCoefficientRule> CreateVariableCoefficientRuleCollection(IReadOnlyCollection<VariableCoefficient> collection)
        {
            var orderedByOffsetCollection = collection
                                                .OrderBy(_ => _.OffsetHours)
                                                .ToArray();

            var ruleCollection = new List<VariableCoefficientRule>(orderedByOffsetCollection.Length);

            for (int index = 0; index < orderedByOffsetCollection.Length; index++)
            {
                var nextCoefficientIndex = index + 1;
                var from = orderedByOffsetCollection[index];
                var to = nextCoefficientIndex < orderedByOffsetCollection.Length ? orderedByOffsetCollection[nextCoefficientIndex] : null;

                ruleCollection.Add(new VariableCoefficientRule(from, to));
            }

            return ruleCollection.ToArray();
        }
    }
}