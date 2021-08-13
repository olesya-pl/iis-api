using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Add_Editor_To_MaterialEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EditorId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_EditorId",
                table: "Materials",
                column: "EditorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Users_EditorId",
                table: "Materials",
                column: "EditorId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Users_EditorId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_EditorId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "EditorId",
                table: "Materials");
        }
    }
}
