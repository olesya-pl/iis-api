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
        private readonly IOntologyProvider _ontologyProvider;

        public Query([Service] IOntologyProvider ontologyProvider)
        {
            _ontologyProvider = ontologyProvider?? throw new ArgumentNullException(nameof(ontologyProvider));
        }

        [GraphQLNonNullType]
        public EntityTypeCollection GetEntityTypes(EntityTypesFilter filter = null) =>
            new EntityTypeCollection(_ontologyProvider.GetTypes().OfType<Ontology.EntityType>());

        public EntityType GetEntityType([GraphQLNonNullType] string code)
        {
            return new EntityType(_ontologyProvider.GetTypes().SingleOrDefault(t => t.Name == code));
        }
    }
}