using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialChannelMapping : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialChannelMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ChannelName = table.Column<string>(nullable: true),
                    RoleId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialChannelMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialChannelMappings_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialChannelMappings_RoleId",
                table: "MaterialChannelMappings",
                column: "RoleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialChannelMappings");
        }
    }
}
