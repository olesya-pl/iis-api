using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class UpdatedTowerLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TowerLocations",
                table: "TowerLocations");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TowerLocations");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "TowerLocations",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "TowerLocations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RadioType",
                table: "TowerLocations",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Range",
                table: "TowerLocations",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Updated",
                table: "TowerLocations",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Created",
                table: "TowerLocations");

            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "TowerLocations");

            migrationBuilder.DropColumn(
                name: "RadioType",
                table: "TowerLocations");

            migrationBuilder.DropColumn(
                name: "Range",
                table: "TowerLocations");

            migrationBuilder.DropColumn(
                name: "Updated",
                table: "TowerLocations");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TowerLocations",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TowerLocations",
                table: "TowerLocations",
                column: "Id");
        }
    }
}
