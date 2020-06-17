using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class ChangeHistoryItemGroup
    {
        [GraphQLType(typeof(IdType))]
        public Guid RequestId { get; set; }

        public List<ChangeHistoryItem> Items { get; set; }
    }
}
