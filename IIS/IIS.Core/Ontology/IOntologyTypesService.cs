using System.Collections.Generic;
using System.Linq;
using IIS.Core.GraphQL;

namespace IIS.Core.Ontology
{
    public interface IOntologyTypesService
    {
        IEnumerable<Type> Types { get; }
        IEnumerable<EntityType> EntityTypes { get; }
        IEnumerable<Type> GetChildTypes(Type parent);
        EntityType GetEntityType(string name);
    }

    public class OntologyTypesService : IOntologyTypesService
    {
        private readonly IOntologyProvider _ontologyProvider;
        private IEnumerable<Type> _types;
        private Dictionary<string, List<Type>> _inheritors;
        private Dictionary<string, EntityType> _entityTypes;

        public IEnumerable<Type> Types => _types ?? (_types = _ontologyProvider.GetTypesAsync().Result.ToList());
        public Dictionary<string, List<Type>> Inheritors => _inheritors ?? (_inheritors = BuildInheritors(Types));

        public IEnumerable<EntityType> EntityTypes =>
            _entityTypes?.Values ?? (_entityTypes = Types.OfType<EntityType>().ToDictionary(t => t.Name)).Values;

        public OntologyTypesService(IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider;
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

        public IEnumerable<Type> GetChildTypes(Type parent) => Inheritors.GetOrDefault(parent.Name);

        public EntityType GetEntityType(string name)
        {
            _entityTypes.TryGetValue(name, out var value);
            return value;
        }
    }
}
