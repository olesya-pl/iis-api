using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class NodeTypeHierarchy : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHierarchyParent",
                table: "NodeTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SecurityStrategy",
                table: "NodeTypes",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHierarchyParent",
                table: "NodeTypes");

            migrationBuilder.DropColumn(
                name: "SecurityStrategy",
                table: "NodeTypes");
        }
    }
}
