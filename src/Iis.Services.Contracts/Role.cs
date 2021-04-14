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
        public AccessGrantedList AllowedItems { get; set; }
        public List<AccessGranted> Tabs => AllowedItems
            .Where(ag => ag.Category == AccessCategory.Tab)
            .Select(p => p.AddGranted(AccessGrantedItems.FirstOrDefault(granted => granted.Id == p.Id)))
            .ToList();
        public List<AccessGranted> Entities => AllowedItems
            .Where(ag => ag.Category == AccessCategory.Entity)
            .Select(p => p.AddGranted(AccessGrantedItems.FirstOrDefault(granted => granted.Id == p.Id)))
            .ToList();
    }
}
