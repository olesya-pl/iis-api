using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {
        private IOntologyProvider _ontologyProvider;

        public Query([Service] IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider?? throw new ArgumentNullException(nameof(ontologyProvider));
            
        }

        // Query should depend on OntologyRepository. Delete this method after.
        private IEnumerable<Type> GetTypes()
        {
            var task = _ontologyProvider.GetTypesAsync();
            task.Wait();
            return task.Result;
        }
        
        [GraphQLNonNullType]
        public EntityTypeCollection GetEntityTypes(EntityTypesFilter filter = null) => new EntityTypeCollection(GetTypes());

        public EntityType GetEntityType([GraphQLNonNullType] string code)
        {
            return new EntityType(GetTypes().SingleOrDefault(t => t.Name == code));
        }
    }
}