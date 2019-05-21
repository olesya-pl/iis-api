using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Ontology.EntityFramework
{
    public partial class TemporaryFile
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public JObject Meta { get; set; }
        public byte[] File { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
