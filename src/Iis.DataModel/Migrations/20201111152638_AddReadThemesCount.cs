using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddReadThemesCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReadQueryResults",
                table: "Themes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("update public.\"Themes\" set \"ReadQueryResults\" = \"QueryResults\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadQueryResults",
                table: "Themes");
        }
    }
}
