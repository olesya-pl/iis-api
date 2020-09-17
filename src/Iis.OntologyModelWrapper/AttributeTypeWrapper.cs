using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class AttributeTypeWrapper : NodeTypeWrapper, IAttributeTypeModel
    {
        public AttributeTypeWrapper(INodeTypeLinked source) : base(source) { }
        public ScalarType ScalarTypeEnum => _source.AttributeType.ScalarType;

        public bool AcceptsScalar(object value)
        {
            return (value is int || value is long) && ScalarTypeEnum == ScalarType.Int
                || value is bool && ScalarTypeEnum == ScalarType.Boolean
                || value is decimal && ScalarTypeEnum == ScalarType.Decimal
                || value is string && ScalarTypeEnum == ScalarType.String
                || value is string && ScalarTypeEnum == ScalarType.IntegerRange
                || value is string && ScalarTypeEnum == ScalarType.FloatRange
                || value is DateTime && ScalarTypeEnum == ScalarType.Date
                || value is Dictionary<string, object> && ScalarTypeEnum == ScalarType.Geo
                || value is Guid && ScalarTypeEnum == ScalarType.File;
        }
    }
}
