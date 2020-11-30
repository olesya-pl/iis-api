using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class ExtendedThemeEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Meta",
                table: "Themes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QueryRequest",
                table: "Themes",
                type: "jsonb",
                nullable: true);

            migrationBuilder.Sql("update public.\"Themes\" set \"QueryRequest\" = json_build_object('suggestion', \"Query\")");

            migrationBuilder.DropColumn(
                name: "Query",
                table: "Themes");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Query",
                table: "Themes",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("update public.\"Themes\" set \"Query\" = jsonb_extract_path_text(\"QueryRequest\", 'suggestion')");

            migrationBuilder.DropColumn(
                name: "Meta",
                table: "Themes");

            migrationBuilder.DropColumn(
                name: "QueryRequest",
                table: "Themes");

            
        }
    }
}
