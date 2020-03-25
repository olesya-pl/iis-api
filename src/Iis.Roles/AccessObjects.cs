using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Iis.Roles
{
    internal class AccessObjects
    {
        public IReadOnlyList<IAccessObject> Items { get; private set; }
        public AccessObjects()
        {
            Items = new List<IAccessObject>
            {
                new AccessObject("ДОР", AccessKind.Dor, AccessCategory.Entity, "CRUD"),
                new AccessObject("Матер", AccessKind.Dor, AccessCategory.Entity, "CRUD"),
            };
        }
    }
}
