using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology;
using Iis.Domain;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public async Task<EntityTypeCollection> GetEntityTypes([Service]IOntologyProvider ontologyProvider,
            EntityTypesFilter filter = null)
        {
            var ontology = await ontologyProvider.GetOntologyAsync();
            IEnumerable<NodeType> types;
            if (filter != null)
            {
                var et = ontology.GetEntityType(filter.Parent);
                if (et == null)
                    types = new List<NodeType>();
                else
                    types = ontology.GetChildTypes(et);
                if (filter.ConcreteTypes)
                    types = types.OfType<Iis.Domain.EntityType>().Where(t => !t.IsAbstract);
            }
            else
            {
                types = ontology.EntityTypes;
            }
            return new EntityTypeCollection(types, ontology);
        }

        public async Task<EntityType> GetEntityType([Service]IOntologyProvider ontologyProvider,
            [GraphQLNonNullType] string code)
        {
            var ontology = await ontologyProvider.GetOntologyAsync();
            var type = ontology.GetEntityType(code);
            return type == null ? null : new EntityType(type, ontology);
        }
    }
}
