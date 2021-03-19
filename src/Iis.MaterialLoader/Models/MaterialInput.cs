using System;
using System.Collections.Generic;
using Iis.Interfaces.Enums;

namespace Iis.MaterialLoader.Models
{
    public class MaterialInput
    {
        public string Metadata { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FileId { get; set; }
        public IEnumerable<Data> Data { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Content { get; set; }
        public string ImportanceText { get; set; }
        public string ReliabilityText { get; set; }
        public string SourceReliabilityText { get; set; }
        public string RelevanceText { get; set; }
        public string CompletenessText { get; set; }
        public IEnumerable<string> Objects { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public IEnumerable<string> States { get; set; }
        public string Code { get; set; }
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public Guid? DocumentId { get; set; }
        public DateTime? CreationDate { get; set; }
        public IEnumerable<DateTime> ModificationDates { get; set; }
        public string Coordinates { get; set; }
        public int AccessLevel { get; set; }
    }
}
