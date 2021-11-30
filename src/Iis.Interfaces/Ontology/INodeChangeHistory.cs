using Iis.Interfaces.Enums;
using System;

namespace Iis.Interfaces.Ontology
{
    public interface INodeChangeHistory
    {
        Guid TargetId { get; }
        string UserName { get; }
        string PropertyName { get; }
        DateTime Date { get; }
        string OldValue { get; }
        string NewValue { get; }
        Guid RequestId { get; }
        ChangeHistoryEntityType Type { get; }
        string ParentTypeName { get; }
        string OldTitle { get; }
        string NewTitle { get; }
    }
}