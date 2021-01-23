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
        public Task<EntityTypeCollection> GetEntityTypes([Service]IOntologyModel ontology,
            EntityTypesFilter filter = null)
        {
            IEnumerable<INodeTypeLinked> types;
            if (!string.IsNullOrEmpty(filter?.Parent))
            {
                var et = ontology.GetEntityType(filter.Parent);
                if (et == null)
                    types = new List<INodeTypeLinked>();
                else
                    types = ontology.GetChildTypes(et);                
            }
            else
            {
                types = ontology.EntityTypes;
            }
            if (filter?.ConcreteTypes == true)
            {
                types = types.OfType<INodeTypeLinked>().Where(t => !t.IsAbstract);
            }                
            return Task.FromResult(new EntityTypeCollection(types, ontology));
        }

        public Task<EntityType> GetEntityType([Service]IOntologyModel ontology,
            [GraphQLNonNullType] string code)
        {
            var type = ontology.GetEntityType(code);
            return Task.FromResult(type == null ? null : new EntityType(type, ontology));
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
