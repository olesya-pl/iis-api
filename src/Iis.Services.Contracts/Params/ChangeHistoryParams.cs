using System;

namespace Iis.Services.Contracts.Params
{
    public class ChangeHistoryParams
    {
        public Guid TargetId { get; set; }
        public string PropertyName { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool ApplyAliases { get; set; }
    }
}
