using Iis.Domain;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyModelWrapper.Meta;
using Iis.OntologySchema.DataTypes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class NodeTypeWrapper : INodeTypeModel
    {
        protected INodeTypeLinked _source;
        public INodeTypeLinked Source => _source;
        public NodeTypeWrapper(INodeTypeLinked source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            _source = source;
        }

        public IEnumerable<INodeTypeModel> AllParents => _source.GetAllAncestors().Select(nt => new NodeTypeWrapper(nt));

        public IEnumerable<INodeTypeModel> AllProperties => _source.GetAllProperties().Select(nt => new NodeTypeWrapper(nt));

        public DateTime CreatedAt => _source.CreatedAt;

        public IEnumerable<INodeTypeModel> DirectParents => _source.DirectParents.Select(nt => new NodeTypeWrapper(nt));

        public IEnumerable<INodeTypeModel> DirectProperties => _source.DirectProperties.Select(nt => new NodeTypeWrapper(nt));

        public bool HasUniqueValues => _source.HasUniqueValues;
        public string UniqueValueFieldName => _source.UniqueValueFieldName;
        public Guid Id => _source.Id;

        public bool IsObjectOfStudy => _source.IsObjectOfStudy;
        public bool IsAbstract => _source.IsAbstract;

        public ISchemaMeta Meta => _source.MetaObject;
        public JObject MetaSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => _source.Name;
        public override string ToString() => Name;

        public string Title => _source.Title;

        public DateTime UpdatedAt => _source.UpdatedAt;
        public bool IsComputed => !string.IsNullOrWhiteSpace(_source.MetaObject?.Formula);

        public INodeTypeModel GetProperty(string typeName)
        {
            return AllProperties.FirstOrDefault(p => p.Name == typeName);
        }

        public bool IsSubtypeOf(INodeTypeModel type)
        {
            return Id == type.Id || _source.IsInheritedFrom(type.Name);
        }

        public ScalarType ScalarTypeEnum => _source.AttributeType?.ScalarType ?? default;

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


        public ISchemaMeta EmbeddingMeta => _source.MetaObject;

        public EmbeddingOptions EmbeddingOptions => _source.RelationType.EmbeddingOptions;

        public INodeTypeModel EntityType =>
            _source.RelationType.TargetType.Kind == Kind.Entity ?
                    new NodeTypeWrapper(_source.RelationType.TargetType) :
                    null;

        public INodeTypeModel AttributeType =>
            _source.RelationType.TargetType.Kind == Kind.Attribute ?
                new NodeTypeWrapper(_source.RelationType.TargetType) :
                null;

        public bool IsAttributeType => _source.RelationType.TargetType.Kind == Kind.Attribute;

        public bool IsEntityType => _source.RelationType.TargetType.Kind == Kind.Entity;

        public bool IsInversed => _source.IsInversed;

        public INodeTypeModel TargetType =>
            _source?.RelationType.TargetType == null ?
                null :
                new NodeTypeWrapper(_source.RelationType.TargetType);

        public bool AcceptsOperation(EntityOperation create) => true;
    }
}
