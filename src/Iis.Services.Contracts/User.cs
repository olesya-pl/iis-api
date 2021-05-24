using System;
using System.Collections.Generic;
using System.Linq;
using Iis.Interfaces.Enums;
using Iis.Interfaces.Roles;
using Iis.Interfaces.Users;

namespace Iis.Services.Contracts
{
    public class User
    {
        public Guid Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public string Comment { get; set; }
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string UserNameActiveDirectory { get; set; }
        public bool IsBlocked { get; set; }
        public bool IsAdmin { get; set; }
        public int AccessLevel { get; set; }
        public UserSource Source { get; set; }
        public IEnumerable<Role> Roles {get;set;} = new List<Role>();
        public AccessGrantedList AccessGrantedItems { get; set; } = new AccessGrantedList();
        public List<AccessGranted> Tabs => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Tab).ToList();
        public List<AccessGranted> Entities => AccessGrantedItems.Where(ag => ag.Category == AccessCategory.Entity).ToList();
        public bool IsGranted(AccessKind kind, AccessOperation operation, AccessCategory category)
        {
            return IsAdmin || AccessGrantedItems.IsGranted(kind, operation, category);
        }
        public bool IsEntityReadGranted() => IsGranted(AccessKind.Entity, AccessOperation.Read, AccessCategory.Entity);
        public bool IsEntitySearchGranted() => IsGranted(AccessKind.Entity, AccessOperation.Search, AccessCategory.Entity);
        public bool IsWikiReadGranted() => IsGranted(AccessKind.Wiki, AccessOperation.Read, AccessCategory.Entity);
        public bool IsWikiSearchGranted() => IsGranted(AccessKind.Wiki, AccessOperation.Search, AccessCategory.Entity);
    }
}
