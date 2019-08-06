using System.Collections.Generic;
using System.Linq;
using IIS.Core.GraphQL;

namespace IIS.Core.Ontology
{
    public interface IOntologyRepository
    {
        IEnumerable<Type> Types { get; }
        IEnumerable<EntityType> EntityTypes { get; }
        IEnumerable<Type> GetChildTypes(Type parent);
        EntityType GetEntityType(string name);
    }

    public class OntologyRepository : IOntologyRepository
    {
        public IEnumerable<Type> Types { get; }
        public IEnumerable<EntityType> EntityTypes { get; }

        public OntologyRepository(IOntologyProvider ontologyProvider)
        {
            var task = ontologyProvider.GetTypesAsync();
            task.Wait();
            Types = task.Result;
            EntityTypes = Types.OfType<EntityType>().ToList();
        }
        
        public IEnumerable<Type> GetChildTypes(Type parent) =>
            Types.Where(t => t.Nodes.OfType<InheritanceRelationType>().Any(r => r.ParentType.Name == parent.Name));

        public EntityType GetEntityType(string name) => EntityTypes.SingleOrDefault(t => t.Name == name);
    }
}