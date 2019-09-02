using System;
using Newtonsoft.Json.Linq;

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

        public bool AcceptsScalar(object value)
        {
            return value is int && ScalarTypeEnum == ScalarType.Integer
                || value is bool && ScalarTypeEnum == ScalarType.Boolean
                || value is decimal && ScalarTypeEnum == ScalarType.Decimal
                || value is string && ScalarTypeEnum == ScalarType.String
                || value is DateTime && ScalarTypeEnum == ScalarType.DateTime
                || value is JObject && ScalarTypeEnum == ScalarType.Geo
                || value is Guid && ScalarTypeEnum == ScalarType.File;
        }

        public static object ParseValue(string value, ScalarType scalarType)
        {
            switch (scalarType)
            {
                case Core.Ontology.ScalarType.Boolean: return bool.Parse(value);
                case Core.Ontology.ScalarType.DateTime: return DateTime.Parse(value);
                case Core.Ontology.ScalarType.Decimal: return decimal.Parse(value);
                case Core.Ontology.ScalarType.Integer: return int.Parse(value);
                case Core.Ontology.ScalarType.String: return value;
                //case Core.Ontology.ScalarType.Json: return value;
                case Core.Ontology.ScalarType.Geo: return JObject.Parse(value);
                case Core.Ontology.ScalarType.File: return Guid.Parse(value);
                default: throw new NotImplementedException();
            }
        }
    }
}
