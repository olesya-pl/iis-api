using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using HotChocolate;
using HotChocolate.Types;
using Iis.Api.GraphQL.Entities;
using Iis.Api.GraphQL.Entities.ObjectTypes;
using Iis.Interfaces.Materials;
using Iis.Services.Contracts.Interfaces;
using IIS.Core.GraphQL.Scalars;
using IIS.Core.GraphQL.Users;
using Newtonsoft.Json.Linq;
using FileInfo = IIS.Core.GraphQL.Files.FileInfo;
using Iis.Api.GraphQL.Common;

namespace IIS.Core.GraphQL.Materials
{
    public class Material : IMaterialLoadData
    {
        [GraphQLType(typeof(NonNullType<IdType>))]
        public Guid Id { get; set; }
        [GraphQLIgnore]
        public Guid? FileId { get; set; }
        [GraphQLNonNullType, GraphQLType(typeof(JsonScalarType))]
        public JObject Metadata { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string CreatedDate { get; set; }
        public string UpdatedAt { get; set; }
        public string Content { get; set; }
        public MaterialSign Importance { get; set; }
        public MaterialSign Reliability { get; set; }
        public MaterialSign Relevance { get; set; }
        public MaterialSign Completeness { get; set; }
        public MaterialSign SourceReliability { get; set; }
        public MaterialSign ProcessedStatus { get; set; }
        public MaterialSign SessionPriority { get; set; }
        public IEnumerable<Data> Data { get; set; }
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Transcriptions { get; set; }
        public string Title { get; set; }
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public string Coordinates { get; set; }
        public string Code { get; set; }
        [GraphQLType(typeof(PredictableDateType))]
        public DateTime? ReceivingDate { get; set; }
        public IEnumerable<string> Objects { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<string> States { get; set; } = new List<string>();
        public IEnumerable<Material> Children { get; set; } = new List<Material>();
        [GraphQLType(typeof(JsonScalarType))]
        public JToken Highlight { get; set; }
        [GraphQLType(typeof(JsonScalarType))]
        public JObject ObjectsOfStudy { get; set; }
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Events { get; set; }
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Features { get; set; }
        public IReadOnlyCollection<User> Assignees { get; set; } = Array.Empty<User>();
        public User Editor { get; set; }
        public int MlHandlersCount { get; set; }
        public int ProcessedMlHandlersCount { get; set; }
        public bool CanBeEdited { get; set; }
        public int AccessLevel { get; set; }
        public IdTitle Caller { get; set; }
        public IdTitle Receiver { get; set; }
        public string RegistrationDate { get; set; }
        public IReadOnlyCollection<GeoCoordinate> CoordinateList { get; set; }
        public IReadOnlyCollection<RelatedObjectOfStudy> RelatedObjectCollection { get; set; } = Array.Empty<RelatedObjectOfStudy>();
        public IReadOnlyCollection<RelatedObject> RelatedEventCollection { get; set; } = Array.Empty<RelatedObject>();
        public IReadOnlyCollection<RelatedObject> RelatedSignCollection { get; set; } = Array.Empty<RelatedObject>();
        public async Task<FileInfo> GetFile([Service] IFileService fileService)
        {
            if (FileId == null) return null;
            var f = await fileService.GetFileAsync(FileId.Value);
            return f.ToView();
        }
    }
}