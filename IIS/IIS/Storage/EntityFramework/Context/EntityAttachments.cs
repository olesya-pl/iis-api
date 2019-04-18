using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityAttachments
    {
        public long Id { get; set; }
        public byte[] File { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
