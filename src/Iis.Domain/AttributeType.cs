using System;
using System.Collections.Generic;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json;

namespace Iis.Domain
{
    public static class AttributeType
    {
        public static object ParseValue(string value, ScalarType scalarType)
        {
            switch (scalarType)
            {
                case ScalarType.Boolean: return bool.Parse(value);
                case ScalarType.Date: return DateTime.Parse(value).ToUniversalTime();
                case ScalarType.Decimal: return decimal.Parse(value);
                case ScalarType.Int: return int.Parse(value);
                case ScalarType.FloatRange:
                case ScalarType.IntegerRange:
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
