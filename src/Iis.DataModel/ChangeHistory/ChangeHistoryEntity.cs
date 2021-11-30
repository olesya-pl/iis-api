using Iis.Interfaces.Enums;
using Iis.Interfaces.Ontology;
using System;

namespace Iis.DataModel.ChangeHistory
{
    public class ChangeHistoryEntity : BaseEntity, INodeChangeHistory
    {
        public Guid TargetId { get; set; }
        public string UserName { get; set; }
        public string PropertyName { get; set; }
        public DateTime Date { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public Guid RequestId { get; set; }
        public ChangeHistoryEntityType Type { get; set; }
        public string ParentTypeName { get; set; }
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
    }
}