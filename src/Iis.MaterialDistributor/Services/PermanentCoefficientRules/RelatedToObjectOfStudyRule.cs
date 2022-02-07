using Iis.MaterialDistributor.Contracts.Services.DataTypes;

namespace Iis.MaterialDistributor.Services
{
    internal class RelatedToObjectOfStudyRule : PermanentCoefficientRule
    {
        public override bool IsSatisfied(MaterialInfo model) => IsJoined(model.Material);
    }
}