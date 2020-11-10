using System;

namespace Iis.DbLayer.Repositories
{
    public class ObjectFeatureRelation
    {
        public Guid ObjectId { get; internal set; }
        public Guid FeatureId { get; internal set; }
    }
}