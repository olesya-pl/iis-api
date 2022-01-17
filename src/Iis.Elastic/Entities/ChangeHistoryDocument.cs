using System;
using System.Collections.Generic;

namespace Iis.Elastic.Entities
{
    public class ChangeHistoryDocument
    {
        public const string MaterialLinkPropertyName = "MaterialLink";

        public Guid Id { get; set; }
        public Guid TargetId { get; set; }
        public string UserName { get; set; }
        public string PropertyName { get; set; }
        public string Date { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public Guid RequestId { get; set; }
        public int Type { get; set; }
        public string ParentTypeName { get; set; }
        public string OldTitle { get; set; }
        public string NewTitle { get; set; }
        public IReadOnlyCollection<Role> Roles { get; set; } = Array.Empty<Role>();
    }

    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}