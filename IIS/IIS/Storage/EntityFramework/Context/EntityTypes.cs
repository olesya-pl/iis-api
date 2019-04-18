using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityTypes
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public bool IsAbstract { get; set; }
        public string Meta { get; set; }
    }
}
