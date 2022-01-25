using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Iis.MaterialDistributor.Contracts.Repositories;
using Iis.MaterialDistributor.DataModel.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Iis.MaterialDistributor.Repositories
{
    public class PermanentCoefficientRepository : IPermanentCoefficientRepository
    {
        private readonly MaterialDistributorContext _dbContext;

        public PermanentCoefficientRepository(MaterialDistributorContext context)
        {
            _dbContext = context;
        }

        public async Task<IReadOnlyDictionary<string, int>> GetAsync(CancellationToken cancellationToken)
        {
            var coefficientDictionary = await _dbContext.PermanentCoefficients
                .AsNoTracking()
                .ToDictionaryAsync(_ => _.Name, _ => _.Value, cancellationToken);

            return coefficientDictionary;
        }
    }
}