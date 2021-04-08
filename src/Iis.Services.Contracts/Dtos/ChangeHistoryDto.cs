using System;

namespace Iis.Services.Contracts.Dtos
{
    public class ChangeHistoryDto
    {
        public DateTime Date { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public string PropertyName { get; set; }
        public Guid TargetId { get; set; }
        public string UserName { get; set; }
        public Guid RequestId { get; set; }
        public int Type { get; set; }
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
        public bool IsCoordinate => NewValue.StartsWith("{\"type\":\"Point\"");
    }
}
