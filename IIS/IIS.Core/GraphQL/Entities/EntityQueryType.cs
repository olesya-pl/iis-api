using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Configuration;
using HotChocolate.Types;
using IIS.Core.GraphQL.ObjectTypeCreators;
using IIS.Core.Ontology;
using IIS.Core.Ontology.Meta;

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
            //_ontologyProvider.GetTypes().ValidateMeta(); // temporary meta validation
            var creator = new ReadQueryTypeCreator(_graphQlTypeProvider);
            descriptor.Name(nameof(EntityQuery));
            descriptor.PopulateFields(_ontologyProvider, creator);
        }
    }
}