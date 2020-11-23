using System;

namespace Iis.DataModel.ChangeHistory
{
    public class ChangeHistoryEntity : BaseEntity
    {
        public Guid TargetId { get; set; }
        public string UserName { get; set; }
        public string PropertyName { get; set; }
        public DateTime Date { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public Guid RequestId { get; set; }
    }
}
