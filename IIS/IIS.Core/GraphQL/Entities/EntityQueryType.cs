using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors.Definitions;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;

namespace IIS.Core.GraphQL.Entities
{
    
    public class EntityQueryType : ObjectType
    {
        private readonly IOntologyProvider _ontologyProvider;
        private readonly IGraphQlOntologyTypeProvider _graphQlOntologyTypeProvider;

        public EntityQueryType(IOntologyProvider ontologyProvider, IGraphQlOntologyTypeProvider graphQlOntologyTypeProvider)
        {
            _ontologyProvider = ontologyProvider ?? throw new ArgumentNullException(nameof(ontologyProvider));
            _graphQlOntologyTypeProvider = graphQlOntologyTypeProvider;
        }
        
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var creator = new EntityTypesCreator(_graphQlOntologyTypeProvider.OntologyTypes);
            var types = _ontologyProvider.GetTypes().OfType<EntityType>();
            descriptor.Name("EntityQuery");
            descriptor.Field("_typesCount").Type<IntType>().Resolver(types.Count());
            foreach (var type in types)
                descriptor.Field(type.Name).Type(creator.CreateObjectType(type)).ResolverNotImplemented();
        }
    }

    // Explicit interface declaration, that would be implemented by each EntityType
    public class EntityInterface : InterfaceType
    {
        protected override void Configure(IInterfaceTypeDescriptor descriptor)
        {
            descriptor.Field("id").Type<IdType>();
        }
    }
}