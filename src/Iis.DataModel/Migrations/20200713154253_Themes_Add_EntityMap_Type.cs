using Microsoft.EntityFrameworkCore.Migrations;

using Iis.DataModel.Themes;

namespace IIS.Core.Migrations
{
    public partial class Themes_Add_EntityMap_Type : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ThemeTypes",
                columns: new[] { "Id", "EntityTypeName", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { ThemeTypeEntity.EntityMapId, "EntityMap", "Мапа", "Мапа" },
                });

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ThemeTypes",
                keyColumn: "Id",
                keyValue: ThemeTypeEntity.EntityMapId);
        }
    }
}
