using System;
using System.Collections.Generic;
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
        public AccessGrantedList AccessGrantedItems { get; set; }
    }
}
