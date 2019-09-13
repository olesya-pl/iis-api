using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology;
using Type = IIS.Core.Ontology.Type;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public async Task<EntityTypeCollection> GetEntityTypes([Service]IOntologyProvider ontologyProvider,
            EntityTypesFilter filter = null)
        {
            var ontology = await ontologyProvider.GetOntologyAsync();
            IEnumerable<Type> types;
            if (filter != null)
            {
                var et = ontology.GetEntityType(filter.Parent);
                if (et == null)
                    types = new List<Type>();
                else
                    types = ontology.GetChildTypes(et);
            }
            else
            {
                types = ontology.EntityTypes;
            }
            return new EntityTypeCollection(types);
        }

        public async Task<EntityType> GetEntityType([Service]IOntologyProvider ontologyProvider,
            [GraphQLNonNullType] string code)
        {
            var ontology = await ontologyProvider.GetOntologyAsync();
            var type = ontology.GetEntityType(code);
            return type == null ? null : new EntityType(type);
        }
    }
}
