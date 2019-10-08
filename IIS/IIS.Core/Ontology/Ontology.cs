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
            try
            {
                return EntityTypes.Where(e => e.Name == name).First();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidOperationException($"Type '{name}' does not exist");
            }
        }

        public Type GetType(Guid id) => Types.SingleOrDefault(e => e.Id == id);

        public IEnumerable<Type> GetChildTypes(Type type) => EntityTypes.Where(etype => etype.IsSubtypeOf(type));
    }
}
