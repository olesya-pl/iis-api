using System.Threading;
using System.Threading.Tasks;
using Iis.MaterialDistributor.DataModel.Entities;

namespace Iis.MaterialDistributor.Contracts.Repositories
{
    public interface IVariableCoefficientRepository
    {
        Task<VariableCoefficientEntity[]> GetAllAsync(CancellationToken cancellationToken);
    }
}