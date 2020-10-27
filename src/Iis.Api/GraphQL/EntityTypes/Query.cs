using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using IIS.Core.Ontology;
using Iis.Domain;
using IIS.Domain;
using Iis.Interfaces.Ontology.Schema;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public Task<EntityTypeCollection> GetEntityTypes([Service]IOntologySchema schema,
            EntityTypesFilter filter = null)
        {
            IEnumerable<INodeTypeLinked> types;
            if (filter != null)
            {
                var node = schema.GetEntityTypeByName(filter.Parent);
                types = node == null ? new List<INodeTypeLinked>()
                    : node.GetAllDescendants();

                if (filter.ConcreteTypes)
                    types = types.Where(nt => !nt.IsAbstract);
            }
            else
            {
                types = schema.GetEntityTypes();
            }

            return Task.FromResult(new EntityTypeCollection(types));
        }

        public Task<EntityType> GetEntityType([Service]IOntologySchema schema,
            [GraphQLNonNullType] string code)
        {
            var type = schema.GetEntityTypeByName(code);
            return Task.FromResult(type == null ? null : new EntityType(type));
        }
    }
}
