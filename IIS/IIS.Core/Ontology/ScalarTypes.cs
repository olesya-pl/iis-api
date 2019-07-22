using System.Collections.Generic;

namespace IIS.Core.Ontology
{
    public enum ScalarType
    {
        String, Integer, Decimal, Boolean, DateTime, Geo, File
    }

    public static class ScalarExtensions
    {
        private static readonly Dictionary<string, ScalarType> _stringToEnum = new Dictionary<string, ScalarType>
        {
            ["int"] = ScalarType.Integer,
            ["integer"] = ScalarType.Integer,
            ["string"] = ScalarType.String,
            ["float"] = ScalarType.Decimal,
            ["decimal"] = ScalarType.Decimal,
            ["bool"] = ScalarType.Boolean,
            ["boolean"] = ScalarType.Boolean,
            ["date"] = ScalarType.DateTime,
            ["datetime"] = ScalarType.DateTime,
            ["geo"] = ScalarType.Geo,
            ["file"] = ScalarType.File,
        };
        
        public static ScalarType ToScalarType(this string scalarTypeString) => _stringToEnum[scalarTypeString.ToLower()];
    }
}