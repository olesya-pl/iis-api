using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ML
{
    public class MachineLearningHadnlersCountResult
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid MaterialId { get; set; }
        public int HandlersCount { get; set; }
    }
}
