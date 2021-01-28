using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.MaterialLoader
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
    }

    public class MaterialLoadData
    {
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public string Coordinates { get; set; }
        public string Code { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public IEnumerable<string> Objects { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<string> States { get; set; } = new List<string>();
    }

    public class Metadata
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime? Date { get; set; }
        public Features Features { get; set; }
    }

    public class Features
    {
        public IEnumerable<Node> Nodes { get; set; }
        public JObject Metadata { get; set; }
    }

    public class Node
    {
        public string Relation { get; set; }
        public string Type { get; set; } // todo confluence
        public string Value { get; set; }
        public JObject Original { get; set; }
        public UpdateFieldData UpdateField { get; set; }
    }

    public class UpdateFieldData
    {
        public string Name { get; set; }

        public List<FieldValue> Values { get; set; }
    }

    public class FieldValue
    {
        public double Lat { get; set; }

        public double Lng { get; set; }

        public DateTime RegisteredAt { get; set; }
    }

    public class Data
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
