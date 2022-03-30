using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Domain.Users;
using Iis.Interfaces.Roles;

namespace Iis.Services.Contracts.Dtos.Roles
{
    public class GroupDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public IReadOnlyCollection<Role> Roles { get; set; } = new List<Role>();
        public AccessGrantedList AccessGrantedItems { get; set; } = new AccessGrantedList();
        public IReadOnlyCollection<AccessGranted> Tabs => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Tab).ToList();
        public IReadOnlyCollection<AccessGranted> Entities => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Entity).ToList();
    }
}