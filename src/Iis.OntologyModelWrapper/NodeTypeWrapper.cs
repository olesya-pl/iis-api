using Iis.Domain;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologyModelWrapper.Meta;
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
            _source = source;
        }

        public IEnumerable<IEntityTypeModel> AllParents => _source.GetAllAncestors().Select(nt => new EntityTypeWrapper(nt));

        public IEnumerable<IEmbeddingRelationTypeModel> AllProperties => _source.GetAllProperties().Select(nt => new EmbeddingRelationTypeWrapper(nt));

        public Type ClrType
        {
            get
            {
                if (this is IEntityTypeModel) return typeof(Entity);
                if (this is IEmbeddingRelationTypeModel) return typeof(Relation);
                return _source.ClrType;
            }
        }

        public DateTime CreatedAt
        {
            get { return _source.CreatedAt; }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<IEntityTypeModel> DirectParents => _source.GetDirectAncestors().Select(nt => new EntityTypeWrapper(nt));

        public IEnumerable<IEmbeddingRelationTypeModel> DirectProperties => _source.GetDirectProperties().Select(nt => new EmbeddingRelationTypeWrapper(nt));

        public bool HasUniqueValues => _source.HasUniqueValues;
        public string UniqueValueFieldName
        {
            get { return _source.UniqueValueFieldName; }
            set { throw new NotImplementedException(); }
        }

        public Guid Id => _source.Id;

        public bool IsObjectOfStudy => _source.IsObjectOfStudy;

        public IMeta Meta
        {
            get 
            {
                return _source.MetaMeta; 
            }
            set { throw new NotImplementedException(); }
        }
        public JObject MetaSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => _source.Name;

        public string Title
        {
            get { return _source.Title; }
            set { throw new NotImplementedException(); }
        }

        public DateTime UpdatedAt
        {
            get { return _source.UpdatedAt; }
            set { throw new NotImplementedException(); }
        }

        public void AddType(INodeTypeModel type)
        {
            throw new NotImplementedException();
        }

        public IEmbeddingRelationTypeModel GetProperty(string typeName)
        {
            return AllProperties.FirstOrDefault(p => p.Name == typeName);
        }

        public bool IsSubtypeOf(INodeTypeModel type)
        {
            return Id == type.Id || _source.IsInheritedFrom(type.Name);
        }
    }
}
