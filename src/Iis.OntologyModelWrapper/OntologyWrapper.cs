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

        public IEnumerable<INodeTypeLinked> EntityTypes => _schema.GetEntityTypes();

        public IEnumerable<INodeTypeLinked> GetChildTypes(INodeTypeLinked type)
        {
            var nodeType = _schema.GetNodeTypeById(type.Id);
            var result = new List<INodeTypeLinked>();
            
            result.AddRange(nodeType.GetAllDescendants().Distinct());
            if (!result.Any(nt => nt.Id == type.Id))
            {
                result.Add(type);
            }
            return result.Distinct();
        }

        public INodeTypeLinked GetEntityType(string name)
        {
            return _schema.GetEntityTypeByName(name);
        }

        public INodeTypeLinked GetType(Guid id)
        {
            var nodeType = _schema.GetNodeTypeById(id);
            
            if(nodeType is null) return null;

            switch (nodeType.Kind)
            {
                case Kind.Entity:
                    return nodeType;
                case Kind.Attribute:
                    return nodeType;
                case Kind.Relation:
                    return nodeType;
            }
            return null;
        }

        public IEnumerable<T> GetTypes<T>(string name) where T : INodeTypeLinked
        {
            var list = new List<INodeTypeLinked>();
            var nodeTypes = _schema.GetAllNodeTypes();
            foreach (var nodeType in nodeTypes)
            {
                if (nodeType.Name == name)
                {
                    list.Add(nodeType);
                    break;
                }
            }
            return list.OfType<T>();
        }
    }
}
