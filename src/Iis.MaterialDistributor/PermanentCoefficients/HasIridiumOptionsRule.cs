using Iis.Interfaces.Constants;
using Iis.MaterialDistributor.Contracts.Services.DataTypes;

namespace Iis.MaterialDistributor.PermanentCoefficients
{
    public class HasIridiumOptionsRule : PermanentCoefficientRule
    {
        public override bool IsSatisfied(MaterialInfo model) => HasFeature(model.Metadata, FeatureFields.Mcc)
            && HasFeature(model.Metadata, FeatureFields.Mnc)
            && HasFeature(model.Metadata, FeatureFields.Lac)
            && HasFeature(model.Metadata, FeatureFields.CellId);
    }
}