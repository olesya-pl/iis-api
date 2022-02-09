using Iis.MaterialDistributor.Contracts.Services.DataTypes;
using Iis.Messages.Materials;

namespace Iis.MaterialDistributor.Services
{
    internal class RelatedAndIgnoredRule : PermanentCoefficientRule
    {
        public override bool IsSatisfied(MaterialInfo model) => IsJoined(model.Material)
            && HasImportance(model.Material, Importance.Ignore);
    }
}