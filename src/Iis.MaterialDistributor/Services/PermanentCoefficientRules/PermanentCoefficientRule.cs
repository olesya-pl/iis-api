using System;
using System.Linq;
using Iis.Interfaces.Constants;
using Iis.MaterialDistributor.Contracts.Services;
using Iis.Messages.Materials;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialDistributor.Services
{
    internal abstract class PermanentCoefficientRule
    {
        public abstract bool IsSatisfied(MaterialInfo model);

        protected static bool IsJoined(Material model) => model.RelatedObjectCollection?.Length > 0;

        protected static bool HasFeature(JObject metadata, string featureName)
        {
            if (metadata == null
                || !metadata.TryGetValue(FeatureFields.FeaturesSection, out var featureSection)) return false;

            foreach (JObject feature in featureSection)
            {
                if (feature.TryGetValue(featureName, StringComparison.OrdinalIgnoreCase, out _))
                {
                    return true;
                }
            }

            return false;
        }

        protected static bool HasImportance(Material model, Importance importance)
        {
            return model.RelatedObjectCollection?.Any(_ => _.Importance.HasValue && importance.HasFlag(_.Importance.Value)) ?? false;
        }
    }
}