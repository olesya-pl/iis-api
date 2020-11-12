using Iis.Services.Contracts;
using System;

namespace Iis.ThemeManagement.Models
{
    public class Theme
    {
        public Guid Id { get; set; }
        public int QueryResults {get;set;}
        public int ReadQueryResults { get; set; }
        public string Title { get; set; }
        public string Query { get; set; }
        public ThemeType Type { get; set; }
        public User User { get; set; }
        public string Comment { get; set; }
    }
}