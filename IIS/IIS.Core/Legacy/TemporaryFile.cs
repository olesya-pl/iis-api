using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace IIS.Legacy.EntityFramework
{
    public partial class TemporaryFile
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public JObject Meta { get; set; }
        public byte[] File { get; set; }
        public DateTime? CreatedAt { get; set; }
        public Guid UserId { get; set; }
    }
}
