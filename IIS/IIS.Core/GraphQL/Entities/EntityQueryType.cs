using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Configuration;
using HotChocolate.Types;
using IIS.Core.GraphQL.Common;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.GraphQL.ObjectTypeCreators.ObjectTypes;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

namespace IIS.Core.GraphQL.Entities
{
    
    public class EntityQueryType : ObjectType
    {
        private readonly IOntologyProvider _ontologyProvider;
        private readonly GraphQlTypeCreator _creator;
        private readonly ReadQueryTypeCreator _readQueryCreator;

        public EntityQueryType(IOntologyProvider ontologyProvider, IGraphQlTypeRepository graphQlTypeRepository, GraphQlTypeCreator creator)
        {
            _ontologyProvider = ontologyProvider;
            _creator = creator;
            _readQueryCreator = new ReadQueryTypeCreator(graphQlTypeRepository, creator);
        }
        
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            descriptor.Name(nameof(EntityQuery));
            var typesToPopulate = _ontologyProvider.GetTypes().OfType<EntityType>()
                .Where(t => t.CreateMeta().ExposeOnApi != false);
            _readQueryCreator.PopulateFields(descriptor, typesToPopulate, Operation.Read);
            descriptor.Field("allEntities")
                .Type(new CollectionType("AllEntities", _creator.GetType<EntityInterface>()))
                .Argument("pagination", d => d.Type<NonNullType<InputObjectType<PaginationInput>>>())
                .Argument("filter", d => d.Type<InputObjectType<FilterInput>>())
                .ResolverNotImplemented();
        }
    }
}