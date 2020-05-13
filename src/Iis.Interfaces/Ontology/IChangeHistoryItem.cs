using System;

namespace Iis.Interfaces.Ontology
{
    public interface IChangeHistoryItem
    {
        DateTime Date { get; set; }
        string NewValue { get; set; }
        string OldValue { get; set; }
        string PropertyName { get; set; }
        Guid TargetId { get; set; }
        string UserName { get; set; }
    }
}