using System;
using System.Collections.Generic;

namespace Iis.DataModel.Materials
{
    public class FileEntity : BaseEntity
    {
        public string Name { get; set; }
        public string ContentType { get; set; }
        public byte[] Contents { get; set; }
        public Guid ContentHash { get; set; }
        public DateTime UploadTime { get; set; }
        public bool IsTemporary { get; set; }

        public List<MaterialEntity> Materials { get; set; }
    }
}
