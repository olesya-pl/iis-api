using System;
using HotChocolate;
using HotChocolate.Types;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class ChangeHistoryItem
    {
        public Guid TargetId { get; set; }
        public string UserName { get; set; }
        public string PropertyName { get; set; }
        public DateTime Date { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        [GraphQLType(typeof(IdType))]
        public Guid RequestId { get; set; }
    }
}
