using System;

namespace Iis.DataModel
{
    public class UserEntity
    {
        public Guid   Id           { get; set; }
        public string Username     { get; set; }
        public string Name         { get; set; }
        public string PasswordHash { get; set; }
        public bool IsBlocked      { get; set; }
    }
}
