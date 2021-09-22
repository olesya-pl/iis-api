using System;
using Iis.Domain.Users;

namespace Iis.Services.Contracts.Dtos
{
    public class ThemeDto
    {
        public Guid Id { get; set; }
        public int QueryResults { get; set; }
        public int ReadQueryResults { get; set; }
        public int UnreadCount { get; set; }
        public string Title { get; set; }
        public string QueryRequest { get; set; }
        public ThemeTypeDto Type { get; set; }
        public User User { get; set; }
        public string Comment { get; set; }
        public string Meta { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}