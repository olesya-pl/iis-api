using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class ChangeHistoryTitles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NewTitle",
                table: "ChangeHistory",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldTitle",
                table: "ChangeHistory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewTitle",
                table: "ChangeHistory");

            migrationBuilder.DropColumn(
                name: "OldTitle",
                table: "ChangeHistory");
        }
    }
}
