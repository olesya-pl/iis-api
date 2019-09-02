using System;
using System.Collections.Generic;
using HotChocolate;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialInput
    {
        public Metadata Metadata { get; set; }
        public Guid? ParentId { get; set; }
        public Guid? FileId { get; set; }
        public string Data { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
    }

    public class Metadata
    {
        public string Type { get; set; }
        public string Source { get; set; }
        public DateTime? Date { get; set; }
        public IEnumerable<Feature> Features { get; set; }
    }

    public class Feature
    {
        public IEnumerable<Node> Nodes { get; set; }
        [GraphQLType(typeof(JsonScalarType))] public JObject Metadata { get; set; }
    }

    public class Node
    {
        public string Relation { get; set; }
        public string Type { get; set; } // todo confluence
        public string Value { get; set; }
        [GraphQLType(typeof(JsonScalarType))] public JObject Original { get; set; }
    }

    public class Data
    {
        public string Type { get; set; }
        public string Text { get; set; }
    }
}
