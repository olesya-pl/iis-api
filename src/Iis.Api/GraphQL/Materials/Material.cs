using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Types;
using Iis.Interfaces.Materials;
using IIS.Core.Files;
using IIS.Core.GraphQL.Scalars;
using Newtonsoft.Json.Linq;
using FileInfo = IIS.Core.GraphQL.Files.FileInfo;

namespace IIS.Core.GraphQL.Materials
{
    public class Material: IMaterialLoadData
    {
        [GraphQLType(typeof(NonNullType<IdType>))] public Guid Id { get; set; }
        [GraphQLIgnore] public Guid? FileId { get; set; }
        [GraphQLNonNullType] public Metadata Metadata { get; set; }
        public DateTime CreatedDate { get; set; }
        public MaterialSign Importance { get; set; }
        public MaterialSign Reliability { get; set; }
        public MaterialSign Relevance { get; set; }
        public MaterialSign Completeness { get; set; }
        public MaterialSign SourceReliability { get; set; }
        public IEnumerable<Data > Data { get; set; }
        [GraphQLType(typeof(ListType<JsonScalarType>))]
        public IEnumerable<JObject> Transcriptions { get; set; }
        public string Title { get; set; }
        public string From { get; set; }
        public string LoadedBy { get; set; }
        public string Coordinates { get; set; }
        public string Code { get; set; }
        public DateTime? ReceivingDate { get; set; }
        public IEnumerable<string> Objects { get; set; } = new List<string>();
        public IEnumerable<string> Tags { get; set; } = new List<string>();
        public IEnumerable<string> States { get; set; } = new List<string>();
        public IEnumerable<Material> Children { get; set; } = new List<Material>();
        public IEnumerable<MaterialInfo> Infos { get; set; } = new List<MaterialInfo>();

        public async Task<FileInfo> GetFile([Service] IFileService fileService)
        {
            if (FileId == null) return null;
            var f = await fileService.GetFileAsync(FileId.Value);
            return f.ToView();
        }
    }
}
