using System;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;

namespace IIS.Core.GraphQL.Materials
{
    public class MaterialInput
    {
        [GraphQLNonNullType] public Metadata Metadata { get; set; }
        [GraphQLType(typeof(IdType))] public Guid? ParentId { get; set; }
        [GraphQLType(typeof(IdType))] public Guid? FileId { get; set; }
        [GraphQLNonNullType] public IEnumerable<Data> Data { get; set; }
    }

    public class Metadata
    {
        [GraphQLNonNullType] public string Type { get; set; }
        [GraphQLNonNullType] public string Source { get; set; }
        public DateTime? Date { get; set; }
        [GraphQLNonNullType] public Features Features { get; set; }
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
    }

    public class Data
    {
        [GraphQLNonNullType] public string Type { get; set; }
        [GraphQLNonNullType] public string Text { get; set; }
    }
}
