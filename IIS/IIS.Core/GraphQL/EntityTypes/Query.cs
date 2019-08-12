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
        private readonly IOntologyRepository _ontologyRepository;

        public Query([Service] IOntologyRepository ontologyProvider)
        {
            _ontologyRepository = ontologyProvider?? throw new ArgumentNullException(nameof(ontologyProvider));
        }

        [GraphQLNonNullType]
        public EntityTypeCollection GetEntityTypes(EntityTypesFilter filter = null) =>
            new EntityTypeCollection(_ontologyRepository.EntityTypes);

        public EntityType GetEntityType([GraphQLNonNullType] string code)
        {
            var type = _ontologyRepository.GetEntityType(code);
            return type == null ? null : new EntityType(type);
        }
    }
}