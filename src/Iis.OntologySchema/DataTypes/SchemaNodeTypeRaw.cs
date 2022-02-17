﻿using System;
using System.Collections.Generic;
using System.Text;
using Iis.Interfaces.Ontology.Schema;

namespace Iis.OntologySchema.DataTypes
{
    public class SchemaNodeTypeRaw: INodeType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Meta { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsArchived { get; set; }
        public Kind Kind { get; set; }
        public bool IsAbstract { get; set; }
        public string UniqueValueFieldName { get; set; }
        public string IconBase64Body { get; set; }
        public bool IsHierarchyParent { get; set; }
        public SecurityStrategy SecurityStrategy { get; set; }
    }
}
