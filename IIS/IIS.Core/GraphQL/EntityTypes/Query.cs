using System;
using System.Collections.Generic;
using HotChocolate;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

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
            IEnumerable<Type> types;
            if (filter != null)
            {
                var et = _ontologyTypesService.GetEntityType(filter.Parent);
                if (et == null)
                    types = new List<Type>();
                else
                    types = _ontologyTypesService.GetChildTypes(et);
            }
            else
            {
                types = _ontologyTypesService.EntityTypes;
            }
            return new EntityTypeCollection(types);
        }

        public EntityType GetEntityType([GraphQLNonNullType] string code)
        {
            var type = _ontologyTypesService.GetEntityType(code);
            return type == null ? null : new EntityType(type);
        }
    }
}
