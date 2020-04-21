using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ML
{
    public class MachineLearningResponseInput
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid MaterialId { get; set; }
        public string HandlerName  { get; set; }
        public string OriginalResponse { get; set; }
    }
}
