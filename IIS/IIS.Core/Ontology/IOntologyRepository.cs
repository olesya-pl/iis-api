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
        private Dictionary<string, EntityType> _entityTypes;
        private Dictionary<string, List<Type>> _inheritors;
        public IEnumerable<Type> Types { get; }
        public IEnumerable<EntityType> EntityTypes => _entityTypes.Values;

        public OntologyRepository(IOntologyProvider ontologyProvider)
        {
            var task = ontologyProvider.GetTypesAsync();
            task.Wait();
            Types = task.Result.ToList();
            _entityTypes = Types.OfType<EntityType>().ToDictionary(t => t.Name);
            _inheritors = BuildInheritors(Types);
        }

        private static Dictionary<string, List<T>> BuildInheritors<T>(IEnumerable<T> entityTypes) where T : Type
        {
            var result = new Dictionary<string, List<T>>();
            foreach (var et in entityTypes)
                foreach (var parent in et.AllParents)
                {
                    if (!result.ContainsKey(parent.Name))
                        result.Add(parent.Name, new List<T>());
                    result[parent.Name].Add(et);
                }

            return result;
        }

        public IEnumerable<Type> GetChildTypes(Type parent) => _inheritors.GetOrDefault(parent.Name);

        public EntityType GetEntityType(string name)
        {
            _entityTypes.TryGetValue(name, out var value);
            return value;
        }
    }
}