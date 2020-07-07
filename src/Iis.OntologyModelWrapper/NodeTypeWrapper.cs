using Iis.Domain;
using Iis.Interfaces.Meta;
using Iis.Interfaces.Ontology.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.OntologyModelWrapper
{
    public class NodeTypeWrapper : INodeTypeModel
    {
        protected INodeTypeLinked _source;
        public NodeTypeWrapper(INodeTypeLinked source)
        {
            _source = source;
        }

        public IEnumerable<IEntityTypeModel> AllParents => throw new NotImplementedException();

        public IEnumerable<IEmbeddingRelationTypeModel> AllProperties => throw new NotImplementedException();

        public Type ClrType => this is IEntityTypeModel ? typeof(Entity) : _source.ClrType;

        public DateTime CreatedAt
        {
            get { return _source.CreatedAt; }
            set { throw new NotImplementedException(); }
        }

        public IEnumerable<IEntityTypeModel> DirectParents => throw new NotImplementedException();

        public IEnumerable<IEmbeddingRelationTypeModel> DirectProperties => throw new NotImplementedException();

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
            get { return _source.MetaMeta; }
            set { throw new NotImplementedException(); }
        }
        public JObject MetaSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => _source.Name;

        public IEnumerable<INodeTypeModel> RelatedTypes => throw new NotImplementedException();

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
            throw new NotImplementedException();
        }

        public bool IsSubtypeOf(INodeTypeModel type)
        {
            throw new NotImplementedException();
        }
    }
}
