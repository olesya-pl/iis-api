using Iis.Interfaces.Roles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Iis.Roles
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public AccessGrantedList AccessGrantedItems { get; set; } = new AccessGrantedList();
        public List<AccessGranted> Tabs => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Tab).ToList();
        public List<AccessGranted> Entities => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Entity).ToList();
        public bool IsGranted(AccessKind kind, AccessOperation operation)
        {
            return IsAdmin || AccessGrantedItems.IsGranted(kind, operation);
        }
    }
}
