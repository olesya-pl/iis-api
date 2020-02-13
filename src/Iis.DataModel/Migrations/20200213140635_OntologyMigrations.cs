using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class OntologyMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OntologyMigrations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OrderNumber = table.Column<int>(nullable: false),
                    StructureBefore = table.Column<string>(nullable: true),
                    StructureAfter = table.Column<string>(nullable: true),
                    MigrationRules = table.Column<string>(nullable: true),
                    Log = table.Column<string>(nullable: true),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    IsSuccess = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OntologyMigrations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OntologyMigrations");
        }
    }
}
