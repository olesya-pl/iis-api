using System.Collections.Generic;

namespace IIS.Core.Ontology
{
    public static class ScalarExtensions
    {
        private static readonly Dictionary<string, Iis.Domain.ScalarType> _stringToEnum = new Dictionary<string, Iis.Domain.ScalarType>
        {
            ["int"] = Iis.Domain.ScalarType.Integer,
            ["integer"] = Iis.Domain.ScalarType.Integer,
            ["string"] = Iis.Domain.ScalarType.String,
            ["float"] = Iis.Domain.ScalarType.Decimal,
            ["decimal"] = Iis.Domain.ScalarType.Decimal,
            ["bool"] = Iis.Domain.ScalarType.Boolean,
            ["boolean"] = Iis.Domain.ScalarType.Boolean,
            ["date"] = Iis.Domain.ScalarType.DateTime,
            ["datetime"] = Iis.Domain.ScalarType.DateTime,
            ["geo"] = Iis.Domain.ScalarType.Geo,
            ["file"] = Iis.Domain.ScalarType.File,
        };
        
        public static Iis.Domain.ScalarType ToScalarType(this string scalarTypeString) => _stringToEnum[scalarTypeString.ToLower()];
    }
}
