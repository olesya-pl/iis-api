using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Iis.Domain.Materials
{
    public class Material
    {
        public Guid Id { get; }
        public JArray Data { get; }
        public JObject Metadata { get; }
        public string Type { get; }
        public string Source { get; }
        public MaterialSign Importance { get; set; }
        public MaterialSign Reliability { get; set; }
        public MaterialSign Relevance { get; set; }
        public MaterialSign Completness { get; set; }
        public MaterialSign SourceReliability { get; set; }
        public ICollection<Material> Children { get; } = new List<Material>();
        public FileInfo File { get; set; }
        public ICollection<MaterialInfo> Infos { get; } = new List<MaterialInfo>();
        public DateTime CreatedDate { get; }
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
