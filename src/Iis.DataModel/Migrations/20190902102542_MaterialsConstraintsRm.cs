using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsConstraintsRm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Files_FileId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Materials_ParentId",
                table: "Materials");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Materials",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "FileId",
                table: "Materials",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Files_FileId",
                table: "Materials",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Materials_ParentId",
                table: "Materials",
                column: "ParentId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Files_FileId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Materials_ParentId",
                table: "Materials");

            migrationBuilder.AlterColumn<Guid>(
                name: "ParentId",
                table: "Materials",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "FileId",
                table: "Materials",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Files_FileId",
                table: "Materials",
                column: "FileId",
                principalTable: "Files",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Materials_ParentId",
                table: "Materials",
                column: "ParentId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
