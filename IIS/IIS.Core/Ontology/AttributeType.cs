using System;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Ontology
{
    public class AttributeType : Type
    {
        public ScalarType ScalarTypeEnum { get; }

        public override System.Type ClrType
        {
            get
            {
                switch (ScalarTypeEnum)
                {
                    case ScalarType.String:
                        return typeof(string);
                    case ScalarType.Integer:
                        return typeof(int);
                    case ScalarType.Decimal:
                        return typeof(decimal);
                    case ScalarType.Boolean:
                        return typeof(bool);
                    case ScalarType.DateTime:
                        return typeof(DateTime);
                    case ScalarType.Geo:
                        return typeof(JObject);
                    case ScalarType.File:
                        return typeof(Guid);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public AttributeType(Guid id, string name, ScalarType scalarType)
            : base(id, name)
        {
            ScalarTypeEnum = scalarType;
        }

        public bool AcceptsScalar(object value)
        {
            return (value is int || value is long) && ScalarTypeEnum == ScalarType.Integer
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
                case ScalarType.Boolean: return bool.Parse(value);
                case ScalarType.DateTime: return DateTime.Parse(value);
                case ScalarType.Decimal: return decimal.Parse(value);
                case ScalarType.Integer: return int.Parse(value);
                case ScalarType.String: return value;
                //case ScalarType.Json: return JObject.Parse(value);
                case ScalarType.Geo: return JObject.Parse(value);
                case ScalarType.File: return Guid.Parse(value);
                default: throw new NotImplementedException();
            }
        }

        public static string ValueToString(object value, ScalarType scalarType)
        {
            return value.ToString();
        }

    }
}
