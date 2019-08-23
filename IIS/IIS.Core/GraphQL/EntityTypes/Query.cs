using System;
using HotChocolate;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {
        private readonly IOntologyTypesService _ontologyTypesService;

        public Query([Service] IOntologyTypesService ontologyProvider)
        {
            _ontologyTypesService = ontologyProvider ?? throw new ArgumentNullException(nameof(ontologyProvider));
        }

        [GraphQLNonNullType]
        public EntityTypeCollection GetEntityTypes(EntityTypesFilter filter = null)
        {
            return new EntityTypeCollection(_ontologyTypesService.EntityTypes);
        }

        public EntityType GetEntityType([GraphQLNonNullType] string code)
        {
            var type = _ontologyTypesService.GetEntityType(code);
            return type == null ? null : new EntityType(type);
        }
    }
}
