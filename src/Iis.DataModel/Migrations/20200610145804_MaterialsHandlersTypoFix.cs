using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsHandlersTypoFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MlHadnlersCount",
                table: "Materials");

            migrationBuilder.AddColumn<int>(
                name: "MlHandlersCount",
                table: "Materials",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MlHandlersCount",
                table: "Materials");

            migrationBuilder.AddColumn<int>(
                name: "MlHadnlersCount",
                table: "Materials",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
