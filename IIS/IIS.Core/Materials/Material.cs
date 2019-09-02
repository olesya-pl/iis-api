using System;
using System.Collections.Generic;
using IIS.Core.Files;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Materials
{
    public class Material
    {
        public Guid Id { get; }
        public JObject Data { get; }
        public string Type { get; }
        public string Source { get; }
        public ICollection<Material> Children { get; } = new List<Material>();
        public FileInfo File { get; set; }
        public ICollection<MaterialInfo> Infos { get; } = new List<MaterialInfo>();

        public Material(Guid id, JObject data, string type, string source)
        {
            Id = id;
            Data = data;
            Type = type;
            Source = source;
        }
    }
}
