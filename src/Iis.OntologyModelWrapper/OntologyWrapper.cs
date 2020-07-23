using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.OntologyModelWrapper
{
    public class OntologyWrapper : IOntologyModel
    {
        IOntologySchema _schema;
        public OntologyWrapper(IOntologySchema schema)
        {
            _schema = schema;
        }
        public IEnumerable<IEntityTypeModel> EntityTypes => _schema.GetEntityTypes().Select(et => new EntityTypeWrapper(et));


        public IEnumerable<INodeTypeModel> GetChildTypes(INodeTypeModel type)
        {
            var nodeType = _schema.GetNodeTypeById(type.Id);
            var result = new List<INodeTypeModel>();
            
            result.AddRange(nodeType.GetAllDescendants().Distinct().Select(nt => new EntityTypeWrapper(nt)));
            if (!result.Any(nt => nt.Id == type.Id))
            {
                result.Add(type);
            }
            return result.Distinct();
        }


        public IEntityTypeModel GetEntityType(string name)
        {
            return new EntityTypeWrapper(_schema.GetEntityTypeByName(name));
        }

        public INodeTypeModel GetType(Guid id)
        {
            var nodeType = _schema.GetNodeTypeById(id);
            switch (nodeType.Kind)
            {
                case Kind.Entity:
                    return new EntityTypeWrapper(nodeType);
                case Kind.Attribute:
                    return new AttributeTypeWrapper(nodeType);
                case Kind.Relation:
                    return new EmbeddingRelationTypeWrapper(nodeType);
            }
            return null;
        }

        public IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeModel
        {
            var list = new List<INodeTypeModel>();
            var nodeTypes = _schema.GetAllNodeTypes();
            foreach (var nodeType in nodeTypes)
            {
                if (nodeType.Name != name) continue;
                switch (nodeType.Kind)
                {
                    case Kind.Entity: 
                        list.Add(new EntityTypeWrapper(nodeType));
                        break;
                    case Kind.Attribute:
                        list.Add(new AttributeTypeWrapper(nodeType));
                        break;
                    case Kind.Relation:
                        list.Add(new EmbeddingRelationTypeWrapper(nodeType));
                        break;
                }
            }
            return list.OfType<T>();
        }
    }
}
