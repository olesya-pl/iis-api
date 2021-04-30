using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities.ObjectTypes;

namespace IIS.Core.GraphQL.ChangeHistory
{
    public class EntityChangeHistory
    {
        [GraphQLType(typeof(IdType))]
        public Guid EntityId { get; set; }
        public IReadOnlyCollection<ChangeHistoryGroup> Groups { get; set; }
    }

    public class ChangeHistoryGroup
    {
        [GraphQLType(typeof(IdType))]
        public Guid RequestId { get; set; }
        public List<ChangeHistoryItem> Items { get; set; }
    }

    public class ChangeHistoryItem
    {
        public Guid EntityId { get; set; }
        public string UserName { get; set; }
        public string PropertyName { get; set; }
        [GraphQLType(typeof(PredictableDateType))]
        public DateTime Date { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        [GraphQLType(typeof(IdType))]
        public Guid RequestId { get; set; }
        public bool IsCoordinate {get;set;}
    }
}
