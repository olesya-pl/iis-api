using Iis.MaterialDistributor.DataStorage;

namespace Iis.MaterialDistributor.Contracts.Services
{
    public interface IFinalRatingEvaluator
    {
        decimal GetFinalRating(MaterialDistributionInfo material, UserDistributionInfo user);
    }
}
