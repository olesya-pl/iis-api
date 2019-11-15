using System;

namespace IIS.Core.Users.EntityFramework
{
    public class User
    {
        public Guid   Id           { get; set; }
        public string Username     { get; set; }
        public string Name         { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBlocked      { get; set; }
    }
}
