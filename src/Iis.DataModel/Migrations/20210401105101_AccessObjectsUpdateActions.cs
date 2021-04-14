using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AccessObjectsUpdateActions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteGranted",
                table: "RoleAccess");

            migrationBuilder.DropColumn(
                name: "DeleteAllowed",
                table: "AccessObjects");

            migrationBuilder.AddColumn<bool>(
                name: "AccessLevelUpdateGranted",
                table: "RoleAccess",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CommentingGranted",
                table: "RoleAccess",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SearchGranted",
                table: "RoleAccess",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AccessLevelUpdateAllowed",
                table: "AccessObjects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CommentingAllowed",
                table: "AccessObjects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SearchAllowed",
                table: "AccessObjects",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessLevelUpdateGranted",
                table: "RoleAccess");

            migrationBuilder.DropColumn(
                name: "CommentingGranted",
                table: "RoleAccess");

            migrationBuilder.DropColumn(
                name: "SearchGranted",
                table: "RoleAccess");

            migrationBuilder.DropColumn(
                name: "AccessLevelUpdateAllowed",
                table: "AccessObjects");

            migrationBuilder.DropColumn(
                name: "CommentingAllowed",
                table: "AccessObjects");

            migrationBuilder.DropColumn(
                name: "SearchAllowed",
                table: "AccessObjects");

            migrationBuilder.AddColumn<bool>(
                name: "DeleteGranted",
                table: "RoleAccess",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DeleteAllowed",
                table: "AccessObjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
