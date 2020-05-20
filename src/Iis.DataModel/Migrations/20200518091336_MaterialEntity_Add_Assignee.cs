using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialEntity_Add_Assignee : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_AssigneeId",
                table: "Materials",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Users_AssigneeId",
                table: "Materials",
                column: "AssigneeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Users_AssigneeId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_AssigneeId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "Materials");
        }
    }
}
