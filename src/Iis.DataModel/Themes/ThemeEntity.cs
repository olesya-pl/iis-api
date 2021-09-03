using System;

namespace Iis.DataModel.Themes
{
    public class ThemeEntity : BaseEntity
    {
        public string Title { get; set; }
        public Guid TypeId { get; set; }
        public ThemeTypeEntity Type { get; set; }
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public string Comment { get; set; }
        public int QueryResults { get; set; }
        public int ReadQueryResults { get; set; }
        public int UnreadCount { get; set; }
        public string Meta { get; set; }
        public string QueryRequest { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}