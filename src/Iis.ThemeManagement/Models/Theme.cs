using System;

using Iis.Roles;

namespace Iis.ThemeManagement.Models
{
    public class Theme
    {
        public Guid Id { get; set; }
        public int QueryResults {get;set;}
        public string Title { get; set; }
        public string Query { get; set; }
        public ThemeType Type { get; set; }
        public User User { get; set; }
        public string Comment { get; set; }
    }
}