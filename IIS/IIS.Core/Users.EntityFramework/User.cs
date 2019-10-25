using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IIS.Core.Users.EntityFramework
{
    public class User
    {
        public Guid   Id           { get; set; }
        public string UserName     { get; set; }
        public string Name         { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBlocked      { get; set; }
    }
}
