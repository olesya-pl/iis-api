﻿using System;
using System.Collections.Generic;

namespace IIS.Storage.EntityFramework.Context
{
    public partial class EntityTypeAttributes
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public int AttributeId { get; set; }
        public string Meta { get; set; }
    }
}
