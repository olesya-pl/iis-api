using System;

namespace Iis.Interfaces.Ontology
{
    public class ObjectFeatureRelation
    {
        public Guid ObjectId { get; }
        public Guid FeatureId { get; }

        public ObjectFeatureRelation(Guid objectId, Guid featureId)
        {
            ObjectId = objectId;
            FeatureId = featureId;
        }
    }
}