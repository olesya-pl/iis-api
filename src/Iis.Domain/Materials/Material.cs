using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Materials
{
    public class Material
    {
        public Guid Id { get; }
        public Guid? ParentId { get; set; }
        public JArray Data { get; }
        public JObject Metadata { get; }
        public string Type { get; }
        public string Source { get; }
        public MaterialSign Importance { get; set; }
        public Guid? ImportanceSignId => Importance?.Id;
        public MaterialSign Reliability { get; set; }
        public Guid? ReliabilitySignId => Reliability?.Id;
        public MaterialSign Relevance { get; set; }
        public Guid? RelevanceSignId => Relevance?.Id;
        public MaterialSign Completeness { get; set; }
        public Guid? CompletenessSignId => Completeness?.Id;
        public MaterialSign SourceReliability { get; set; }
        public Guid? SourceReliabilitySignId => SourceReliability?.Id;
        public List<Material> Children { get; } = new List<Material>();
        public FileInfo File { get; set; }
        public Guid? FileId => File?.Id;
        public List<MaterialInfo> Infos { get; } = new List<MaterialInfo>();
        public DateTime CreatedDate { get; set; }
        public string Title { get; set; }
        public MaterialLoadData LoadData { get; set; }

        public Material(Guid id, JObject metadata, JArray data, string type, string source)
        {
            Id = id;
            Metadata = metadata;
            Data = data;
            Type = type;
            Source = source;
            CreatedDate = DateTime.Now;
        }
    }
}
