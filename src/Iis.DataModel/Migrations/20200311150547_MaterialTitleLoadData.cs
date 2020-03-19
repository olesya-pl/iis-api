using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialTitleLoadData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoadData",
                table: "Materials",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Materials",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoadData",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Materials");
        }
    }
}
