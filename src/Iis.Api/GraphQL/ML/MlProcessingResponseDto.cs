using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ML
{
    public class MlProcessingResponseDto
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid MaterialId { get; set; }

        public string MLHandlerName  { get; set; }
        public string OriginalResponse { get; set; }
    }
}
