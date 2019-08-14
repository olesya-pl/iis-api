using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class RelationEmbeddingOptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArray",
                table: "RelationTypes");

            migrationBuilder.AddColumn<int>(
                name: "EmbeddingOptions",
                table: "RelationTypes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmbeddingOptions",
                table: "RelationTypes");

            migrationBuilder.AddColumn<bool>(
                name: "IsArray",
                table: "RelationTypes",
                nullable: false,
                defaultValue: false);
        }
    }
}
