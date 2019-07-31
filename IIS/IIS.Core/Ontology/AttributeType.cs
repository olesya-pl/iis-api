using System;

namespace IIS.Core.Ontology
{
    public class AttributeType : Type
    {
        public ScalarType ScalarTypeEnum { get; }

        public AttributeType(Guid id, string name, ScalarType scalarType)
            : base(id, name)
        {
            ScalarTypeEnum = scalarType;
        }
    }
}
