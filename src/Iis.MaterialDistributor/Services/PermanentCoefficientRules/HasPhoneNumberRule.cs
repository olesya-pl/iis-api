using Iis.Interfaces.Constants;
using Iis.MaterialDistributor.Contracts.Services;

namespace Iis.MaterialDistributor.Services
{
    internal class HasPhoneNumberRule : PermanentCoefficientRule
    {
        public override bool IsSatisfied(MaterialInfo model) => HasFeature(model.Metadata, FeatureFields.PhoneNumber);
    }
}