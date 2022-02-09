using Iis.Interfaces.Constants;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;

namespace Iis.MaterialDistributor.Services
{
    internal class HasTMSIRule : PermanentCoefficientRule
    {
        public override bool IsSatisfied(MaterialInfo model) => HasFeature(model.Metadata, FeatureFields.TMSI);
    }
}