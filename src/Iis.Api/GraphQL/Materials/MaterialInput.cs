using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Materials;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialInput
    {
        [GraphQLNonNullType] public string Metadata { get; set; }
        [GraphQLType(typeof(IdType))] public Guid? ParentId { get; set; }
        [GraphQLType(typeof(IdType))] public Guid? FileId { get; set; }
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

    public class MaterialLoadData : IMaterialLoadData
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
        [GraphQLNonNullType] public string Type { get; set; }
        [GraphQLNonNullType] public string Source { get; set; }
        public DateTime? Date { get; set; }
        public Features Features { get; set; }
    }

    public class Features
    {
        [GraphQLNonNullType] public IEnumerable<Node> Nodes { get; set; }
        [GraphQLType(typeof(JsonScalarType))] public JObject Metadata { get; set; }
    }

    public class Node
    {
        [GraphQLNonNullType] public string Relation { get; set; }
        [GraphQLNonNullType] public string Type { get; set; } // todo confluence
        [GraphQLNonNullType] public string Value { get; set; }
        [GraphQLType(typeof(JsonScalarType))] public JObject Original { get; set; }
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
        [GraphQLNonNullType] public string Type { get; set; }
        public string Text { get; set; }
    }
}
