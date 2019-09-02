using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials
{
    public class MaterialInfo
    {
        public Guid Id { get; }
        public JObject Data { get; }
        public string Source { get; }
        public string SourceType { get; }
        public string SourceVersion { get; }
        public ICollection<MaterialFeature> Features { get; } = new List<MaterialFeature>();

        public MaterialInfo(Guid id, JObject data, string source, string sourceType, string sourceVersion)
        {
            Id = id;
            Data = data;
            Source = source;
            SourceType = sourceType;
            SourceVersion = sourceVersion;
        }
    }
}
