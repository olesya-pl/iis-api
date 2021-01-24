using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate;
using Iis.Domain;
using Iis.Interfaces.Ontology.Schema;
using Iis.OntologySchema.DataTypes;

namespace IIS.Core.GraphQL.EntityTypes
{
    public class Query
    {

        [GraphQLNonNullType]
        public Task<EntityTypeCollection> GetEntityTypes([Service]IOntologySchema schema,
            EntityTypesFilter filter = null)
        {
            IEnumerable<INodeTypeLinked> types;
            if (!string.IsNullOrEmpty(filter?.Parent))
            {
                var et = schema.GetEntityTypeByName(filter.Parent);
                if (et == null)
                    types = new List<INodeTypeLinked>();
                else
                    types = et.GetAllDescendants().Concat(new[] { et });
            }
            else
            {
                types = schema.GetEntityTypes();
            }
            if (filter?.ConcreteTypes == true)
            {
                types = types.Where(t => !t.IsAbstract);
            }                
            return Task.FromResult(new EntityTypeCollection(types));
        }

        public Task<EntityType> GetEntityType([Service]IOntologySchema schema,
            [GraphQLNonNullType] string code)
        {
            var type = schema.GetEntityTypeByName(code);
            return Task.FromResult(type == null ? null : new EntityType(type));
        }

        public List<EntityTypeIconInfo> GetEntityTypeIcons([Service] IOntologySchema schema)
        {
            return schema.GetEntityTypes()
                .Where(nt => !string.IsNullOrEmpty(nt.IconBase64Body))
                .Select(nt => new EntityTypeIconInfo
                {
                    Name = nt.Name,
                    IconBase64Body = nt.IconBase64Body
                })
                .ToList();
        }
    }
}
