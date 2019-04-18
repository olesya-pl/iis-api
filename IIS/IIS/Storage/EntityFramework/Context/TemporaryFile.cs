using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class TemporaryFile
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string Meta { get; set; }
        public byte[] File { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int UserId { get; set; }
    }
}
