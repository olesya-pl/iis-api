using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Themes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ThemeTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ShortTitle = table.Column<string>(maxLength: 16, nullable: false),
                    Title = table.Column<string>(maxLength: 255, nullable: false),
                    EntityTypeName = table.Column<string>(maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ThemeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Themes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(maxLength: 1024, nullable: false),
                    Query = table.Column<string>(maxLength: 1024, nullable: false),
                    TypeId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Themes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Themes_ThemeTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ThemeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Themes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ThemeTypes",
                columns: new[] { "Id", "EntityTypeName", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { new Guid("2b8fd109-cf4a-4f76-8136-de761da53d20"), "EntityMaterial", "М", "Матеріал" },
                    { new Guid("043ae699-e070-4336-8513-e90c87555c58"), "EntityObject", "О", "Об'єкт" },
                    { new Guid("42f61965-8baa-4026-ab33-0378be8a6c3e"), "EntityEvent", "П", "Подія" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Themes_TypeId",
                table: "Themes",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Themes_UserId",
                table: "Themes",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Themes");

            migrationBuilder.DropTable(
                name: "ThemeTypes");
        }
    }
}
