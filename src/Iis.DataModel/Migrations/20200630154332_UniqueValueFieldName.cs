using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class UniqueValueFieldName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueValueFieldName",
                table: "NodeTypes",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UniqueValueFieldName",
                table: "NodeTypes");
        }
    }
}
