using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public Task<EntityTypeCollection> GetEntityTypes([Service]IOntologyModel ontology,
            EntityTypesFilter filter = null)
        {
            IEnumerable<INodeTypeModel> types;
            if (!string.IsNullOrEmpty(filter?.Parent))
            {
                var et = ontology.GetEntityType(filter.Parent);
                if (et == null)
                    types = new List<INodeTypeModel>();
                else
                    types = ontology.GetChildTypes(et);                
            }
            else
            {
                types = ontology.EntityTypes;
            }
            if (filter.ConcreteTypes)
            {
                types = types.OfType<IEntityTypeModel>().Where(t => !t.IsAbstract);
            }                
            return Task.FromResult(new EntityTypeCollection(types, ontology));
        }

        public Task<EntityType> GetEntityType([Service]IOntologyModel ontology,
            [GraphQLNonNullType] string code)
        {
            var type = ontology.GetEntityType(code);
            return Task.FromResult(type == null ? null : new EntityType(type, ontology));
        }
    }
}
