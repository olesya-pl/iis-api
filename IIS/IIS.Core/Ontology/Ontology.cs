using System;
using System.Collections.Generic;
using System.Linq;

namespace IIS.Core.Ontology
{
    public class Ontology
    {
        public IEnumerable<Type> Types { get; }

        public IEnumerable<EntityType> EntityTypes => Types.OfType<EntityType>();

        public Ontology(IEnumerable<Type> types)
        {
            Types = types;
        }

        // TODO: revert it back
        public EntityType GetEntityType(string name)
        {
            var type = EntityTypes.SingleOrDefault(e => e.Name == name);

            if (type == null) {
                throw new ArgumentException($"Type '{name}' does not exist");
            }

            return type;
        }

        public EntityType GetEntityTypeOrNull(string name) => EntityTypes.SingleOrDefault(e => e.Name == name);

        public Type GetType(Guid id) => Types.SingleOrDefault(e => e.Id == id);

        public IEnumerable<Type> GetChildTypes(Type type) => EntityTypes.Where(etype => etype.IsSubtypeOf(type));
    }
}
