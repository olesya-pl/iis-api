using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Roles
{
    internal class AccessObject : IAccessObject
    {
        public string Title { get; set; }
        public AccessKind Kind { get; set; }
        public AccessCategory Category { get; set; }
        public IAccessOperations AllowedOperations { get; set; }
        public AccessObject(string title, AccessKind kind, AccessCategory category, string allowedOperations)
        {
            Title = title;
            Kind = kind;
            Category = category;
            AllowedOperations = new AccessOperations(allowedOperations);
        }
    }
}
