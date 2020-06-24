using System;
using System.Collections.Generic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;

namespace Iis.Domain
{
    public sealed class AttributeType : NodeType
    {
        public ScalarType ScalarTypeEnum { get; }

        public AttributeType(Guid id, string name, ScalarType scalarType)
            : base(id, name)
        {
            ScalarTypeEnum = scalarType;
        }

        public override Type ClrType
        {
            get
            {
                switch (ScalarTypeEnum)
                {
                    case ScalarType.String:
                        return typeof(string);
                    case ScalarType.Int:
                        return typeof(int);
                    case ScalarType.Decimal:
                        return typeof(decimal);
                    case ScalarType.Boolean:
                        return typeof(bool);
                    case ScalarType.Date:
                        return typeof(DateTime);
                    case ScalarType.Geo:
                        return typeof(Dictionary<string, object>);
                    case ScalarType.File:
                        return typeof(Guid);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public bool AcceptsScalar(object value)
        {
            return (value is int || value is long) && ScalarTypeEnum == ScalarType.Int
                || value is bool && ScalarTypeEnum == ScalarType.Boolean
                || value is decimal && ScalarTypeEnum == ScalarType.Decimal
                || value is string && ScalarTypeEnum == ScalarType.String
                || value is DateTime && ScalarTypeEnum == ScalarType.Date
                || value is Dictionary<string, object> && ScalarTypeEnum == ScalarType.Geo
                || value is Guid && ScalarTypeEnum == ScalarType.File;
        }

        public static object ParseValue(string value, ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Boolean: return bool.Parse(value);
                case ScalarType.Date: return DateTime.Parse(value).ToUniversalTime();
                case ScalarType.Decimal: return decimal.Parse(value);
                case ScalarType.Int: return int.Parse(value);
                case ScalarType.String: return value;
                case ScalarType.Geo: return ValueToDict(value);
                case ScalarType.File: return Guid.Parse(value);
                default: throw new NotImplementedException();
            }
        }

        public static string ValueToString(object value, ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Geo:
                    return JsonConvert.SerializeObject(value);
                case ScalarType.Date:
                    if (value == null)
                        return null;
                    return ((DateTime)value).ToUniversalTime().ToString("o");
                default:
                    return value.ToString();
            }
        }

        public static Dictionary<string, object> ValueToDict(string value)
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(value, new DictionaryConverter());
        }
    }
}
