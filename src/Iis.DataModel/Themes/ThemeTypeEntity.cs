using System;

namespace Iis.DataModel.Themes
{
    public class ThemeTypeEntity : BaseEntity
    {
        public static readonly Guid EntityMapId
            = new Guid("2692d888-6274-482e-a198-02c24436df3f");
        public static readonly Guid EntityMaterialId
            = new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20");
        public static readonly Guid EntityObjectId
            = new Guid("043ae699-e070-4336-8513-e90c87555c58");
        public static readonly Guid EntityEventId
            = new Guid("42f61965-8baa-4026-ab33-0378be8a6c3e");
        public static readonly Guid EntityReportId
            = new Guid("2b4b2a5a-bd2a-4159-839e-02e169fc018c");
        public string ShortTitle { get; set; }
        public string Title { get; set; }
        public string EntityTypeName { get; set; }
    }
}