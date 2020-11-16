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
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            _source = source;
        }

        public IEnumerable<IEntityTypeModel> AllParents => _source.GetAllAncestors().Select(nt => new EntityTypeWrapper(nt));

        public IEnumerable<IEmbeddingRelationTypeModel> AllProperties => _source.GetAllProperties().Select(nt => new EmbeddingRelationTypeWrapper(nt));

        public DateTime CreatedAt => _source.CreatedAt;

        public IEnumerable<IEntityTypeModel> DirectParents => _source.DirectParents.Select(nt => new EntityTypeWrapper(nt));

        public IEnumerable<IEmbeddingRelationTypeModel> DirectProperties => _source.DirectProperties.Select(nt => new EmbeddingRelationTypeWrapper(nt));

        public bool HasUniqueValues => _source.HasUniqueValues;
        public string UniqueValueFieldName => _source.UniqueValueFieldName;
        public Guid Id => _source.Id;

        public bool IsObjectOfStudy => _source.IsObjectOfStudy;

        public IMeta Meta => _source.MetaObject;
        public JObject MetaSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string Name => _source.Name;

        public string Title => _source.Title;

        public DateTime UpdatedAt => _source.UpdatedAt;

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
