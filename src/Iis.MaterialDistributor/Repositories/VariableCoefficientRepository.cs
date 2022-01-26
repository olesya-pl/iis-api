using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Iis.MaterialDistributor.DataModel.Contexts;
using Iis.MaterialDistributor.DataModel.Entities;
using Iis.MaterialDistributor.Contracts.Repositories;

namespace Iis.MaterialDistributor.Repositories
{
    internal class VariableCoefficientRepository : IVariableCoefficientRepository
    {
        private readonly MaterialDistributorContext _dbContext;

        public VariableCoefficientRepository(MaterialDistributorContext context)
        {
            _dbContext = context;
        }

        public Task<VariableCoefficientEntity[]> GetAllAsync(CancellationToken cancellationToken)
        {
            return _dbContext.VariableCoefficients.ToArrayAsync(cancellationToken);
        }
    }
}