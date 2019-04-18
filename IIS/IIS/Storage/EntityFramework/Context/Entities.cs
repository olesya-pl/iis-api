using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class Entities
    {
        public long Id { get; set; }
        public int TypeId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
