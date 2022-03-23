using System;
using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Add_SourceAlias_To_MaterialEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceAliasId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialSourceAliasEntity",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Source = table.Column<string>(maxLength: 50, nullable: false),
                    Alias = table.Column<string>(maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSourceAliasEntity", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SourceAliasId",
                table: "Materials",
                column: "SourceAliasId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSourceAliasEntity_Source",
                table: "MaterialSourceAliasEntity",
                column: "Source",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSourceAliasEntity_SourceAliasId",
                table: "Materials",
                column: "SourceAliasId",
                principalTable: "MaterialSourceAliasEntity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSourceAliasEntity_SourceAliasId",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "MaterialSourceAliasEntity");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SourceAliasId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SourceAliasId",
                table: "Materials");
        }
    }
}
