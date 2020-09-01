using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.Roles;

namespace Iis.Services.Contracts
{
    public class Role
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsAdmin { get; set; } = false;
        public List<Guid> ActiveDirectoryGroupIds { get; set; }
        public AccessGrantedList AccessGrantedItems { get; set; }
        public List<AccessGranted> Tabs => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Tab).ToList();
        public List<AccessGranted> Entities => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Entity).ToList();
    }
}
