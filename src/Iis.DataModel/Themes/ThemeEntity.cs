using System;

namespace Iis.DataModel.Themes
{
    public class ThemeEntity : BaseEntity
    {
        public string Title { get; set; }
        public string Query { get; set; }
        public Guid TypeId { get; set; }
        public ThemeTypeEntity Type { get; set; }
        public Guid UserId { get; set; }
        public UserEntity User { get; set; }
        public string Comment { get; set; }
    }
}