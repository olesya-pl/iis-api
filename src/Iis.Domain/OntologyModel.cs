using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Domain
{
    public class OntologyModel
    {
        public IEnumerable<NodeType> Types { get; }

        public IEnumerable<EntityType> EntityTypes => Types.OfType<EntityType>();

        public OntologyModel(IEnumerable<NodeType> types)
        {
            Types = types;
        }

        public EntityType GetEntityType(string name)
        {
            // TODO: this method is redundant and can be removed
            return GetType<EntityType>(name);
        }

        public T GetType<T>(string name) where T: NodeType
        {
            var type = GetTypeOrNull<T>(name);

            if (type == null) {
                throw new ArgumentException($"Type '{name}' does not exist");
            }

            return type;
        }

        public T GetTypeOrNull<T>(string name) where T: NodeType
        {
            return Types.OfType<T>().SingleOrDefault(type => type.Name == name);
        }

        public IEnumerable<T> GetTypes<T>(string name) where T: NodeType
        {
            // TODO: remove this method. There should not be types with the same name
            //       this is a temporary hack while we have relations with the same name but different Source/Target
            return Types.OfType<T>().Where(type => type.Name == name);
        }

        public NodeType GetType(Guid id) => Types.SingleOrDefault(e => e.Id == id);

        public IEnumerable<NodeType> GetChildTypes(NodeType type) => EntityTypes.Where(etype => etype.IsSubtypeOf(type));
    }
}
