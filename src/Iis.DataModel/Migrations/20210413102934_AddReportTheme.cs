using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddReportTheme : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ThemeTypes",
                columns: new[] { "Id", "EntityTypeName", "ShortTitle", "Title" },
                values: new object[] { new Guid("2b4b2a5a-bd2a-4159-839e-02e169fc018c"), "EntityReport", "З", "Звіт" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ThemeTypes",
                keyColumn: "Id",
                keyValue: new Guid("2b4b2a5a-bd2a-4159-839e-02e169fc018c"));
        }
    }
}
