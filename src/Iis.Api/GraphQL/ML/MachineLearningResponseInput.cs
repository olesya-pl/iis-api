using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ML
{
    public class MachineLearningResponseInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))] 
        public Guid MaterialId { get; set; }

        [GraphQLNonNullType]
        public string HandlerName { get; set; }
        [GraphQLNonNullType]
        public string HandlerVersion { get; set; }
        [GraphQLNonNullType]
        public string OriginalResponse { get; set; }
    }
}
