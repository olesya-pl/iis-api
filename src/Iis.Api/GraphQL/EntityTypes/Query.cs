using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology;
using Iis.Domain;
using IIS.Domain;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public async Task<EntityTypeCollection> GetEntityTypes([Service]IOntologyModel ontology,
            EntityTypesFilter filter = null)
        {
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

        public Task<EntityType> GetEntityType([Service]IOntologyModel ontology,
            [GraphQLNonNullType] string code)
        {
            var type = ontology.GetEntityType(code);
            return Task.FromResult(type == null ? null : new EntityType(type, ontology));
        }
    }
}
