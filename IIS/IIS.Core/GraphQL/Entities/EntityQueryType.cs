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
        private readonly IGraphQlTypeProvider _graphQlTypeProvider;

        public EntityQueryType(IOntologyProvider ontologyProvider, IGraphQlTypeProvider graphQlTypeProvider)
        {
            _ontologyProvider = ontologyProvider;
            _graphQlTypeProvider = graphQlTypeProvider;
        }
        
        protected override void Configure(IObjectTypeDescriptor descriptor)
        {
            var creator = new ReadQueryTypeCreator(_graphQlTypeProvider);
            descriptor.Name(nameof(EntityQuery));
            descriptor.PopulateFields(_ontologyProvider, creator);
        }
    }
}