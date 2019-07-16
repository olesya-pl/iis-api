using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.Entities
{
    
    public class EntityQuery : ObjectType
    {
        /*
        private readonly ISchemaProvider _schemaProvider;

        public EntityQuery([Service] ISchemaProvider schemaProvider)
        {
            _schemaProvider = schemaProvider;
        }
        
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var task = _schemaProvider.GetSchemaAsync();
            task.Wait();
            var schema = task.Result;

            var types = new List<ObjectType>();
            
            foreach (var t in schema.Members.Select(m=>m.Target).OfType<ComplexType>())
            {
                var ot = new ObjectType(d =>
                {
                    d.Name(t.Name);
                    d.Interface<EntityInterface>();
                    foreach (var attr in GetAllMembers(t))
                    {
                        d.Field(attr.Name).Type<StringType>().Resolver(ctx => "dummy-" + attr.Name);
                    }
                });
                descriptor.Field(t.Name + "Entity").Type(ot).Resolver(ctx => t.Name + "Dummy");
                
                types.Add(ot);
            }
            
            // GraphQL Union example
            var union = new UnionType(d =>
            {
                d.Name("AllEntitiesUnion");
                foreach (var type in types)
                    d.Type(type);
            });

            descriptor.Field("AllEntities").Type(union).Resolver(ctx => null);
        }
        
        // Taken from IIS.Search model
        static IEnumerable<Member> GetAllMembers(ComplexType t) =>
            t.Parents.SelectMany(p => p.Members).Concat(t.Members)
                .GroupBy(m => m.Name).Select(g => g.First());
                */
    }

    // Explicit interface declaration, that would be implemented by each EntityType
    public class EntityInterface : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Field("id").Type<StringType>();
        }
    }
}