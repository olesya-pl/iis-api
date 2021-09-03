using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddThemesUnreadCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UnreadCount",
                table: "Themes",
                nullable: false,
                defaultValue: 0);
            
            migrationBuilder.Sql(
                "update public.\"Themes\" set \"UnreadCount\" = CASE WHEN COALESCE(\"QueryResults\",0) - COALESCE(\"ReadQueryResults\",0) > 0 THEN COALESCE(\"QueryResults\",0) - COALESCE(\"ReadQueryResults\",0) ELSE 0 END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnreadCount",
                table: "Themes");
        }
    }
}
