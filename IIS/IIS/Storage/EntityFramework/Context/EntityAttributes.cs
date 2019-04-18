using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityAttributes
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Code { get; set; }
        public string Meta { get; set; }
    }
}
