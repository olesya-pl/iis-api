using System;

namespace Iis.ThemeManagement.Models
{
    public class ThemeType
    {
        public Guid Id { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public string EntityTypeName { get; set; }
    }
}