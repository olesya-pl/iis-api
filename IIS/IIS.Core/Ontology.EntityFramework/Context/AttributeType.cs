using System;

namespace IIS.Core.Ontology.EntityFramework.Context
{
    public class AttributeType
    {
        public Guid Id { get; set; }
        public ScalarType ScalarType { get; set; }

        public virtual Type Type { get; set; }
    }

    public enum ScalarType
    {
        String,
        Int,
        Decimal,
        Date,
        Boolean,
        Geo,
        File,
        Json
    }
}
