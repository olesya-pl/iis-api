using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ML
{
    public class MachineLearningResult
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid MaterialId { get; set; }
        public string HandlerName { get; set; }
        public string HandlerVersion { get; set; }
        public string ProcessingDateTime { get; set; }
        public string OriginalResponse { get; set; }
    }
}
