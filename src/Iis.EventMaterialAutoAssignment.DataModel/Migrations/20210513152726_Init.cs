using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Iis.EventMaterialAutoAssignment.DataModel.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AccessLevel = table.Column<string>(maxLength: 127, nullable: true),
                    Component = table.Column<string>(maxLength: 1023, nullable: true),
                    EventType = table.Column<string>(maxLength: 1023, nullable: true),
                    Importance = table.Column<string>(maxLength: 127, nullable: true),
                    Name = table.Column<string>(maxLength: 255, nullable: true),
                    RelatesToCountry = table.Column<string>(maxLength: 127, nullable: true),
                    State = table.Column<string>(maxLength: 127, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentConfigKeywords",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Keyword = table.Column<string>(nullable: true),
                    AssignmentConfigId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentConfigKeywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentConfigKeywords_AssignmentConfigs_AssignmentConfig~",
                        column: x => x.AssignmentConfigId,
                        principalTable: "AssignmentConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentConfigKeywords_AssignmentConfigId",
                table: "AssignmentConfigKeywords",
                column: "AssignmentConfigId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentConfigKeywords");

            migrationBuilder.DropTable(
                name: "AssignmentConfigs");
        }
    }
}
