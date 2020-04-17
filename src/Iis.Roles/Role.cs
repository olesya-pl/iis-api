using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iis.Roles
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAdmin { get; set; } = false;
        public string AdGroup { get; set; }
        public AccessGrantedList AccessGrantedItems { get; set; }
        public List<AccessGranted> Tabs => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Tab).ToList();
        public List<AccessGranted> Entities => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Entity).ToList();
    }
}
