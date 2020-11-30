using System;

namespace Iis.Services.Contracts.Dtos
{
    public class ThemeTypeDto
    {
        public Guid Id { get; set; }
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public string EntityTypeName { get; set; }
    }
}