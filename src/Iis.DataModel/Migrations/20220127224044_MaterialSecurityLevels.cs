using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialSecurityLevels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialSecurityLevels",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MaterialId = table.Column<Guid>(nullable: false),
                    SecurityLevelIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSecurityLevels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialSecurityLevels_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSecurityLevels_MaterialId",
                table: "MaterialSecurityLevels",
                column: "MaterialId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialSecurityLevels");
        }
    }
}
