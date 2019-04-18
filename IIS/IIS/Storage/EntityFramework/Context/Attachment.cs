using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class Attachment
    {
        public int Id { get; set; }
        public byte[] File { get; set; }
    }
}
