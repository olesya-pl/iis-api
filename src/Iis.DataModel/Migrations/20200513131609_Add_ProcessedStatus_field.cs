using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Add_ProcessedStatus_field : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MaterialSignTypes",
                columns: new[] { "Id", "Name", "Title" },
                values: new object[,]
                {
                    { new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"), "ProcessingStatus", "Обробка" },
                });

            migrationBuilder.InsertData(
                table: "MaterialSigns",
                columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { new Guid("c85a76f4-3c04-46f7-aed9-f865243b058e"), new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"), 1, "1", "Оброблено" },
                    { new Guid("0a641312-abb7-4b40-a766-0781308eb077"), new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"), 2, "2", "Не оброблено" },
                });

            migrationBuilder.DropColumn(
                name: "IsProcessed",
                table: "Materials");

            migrationBuilder.AddColumn<Guid>(
                name: "ProcessedStatusSignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ProcessedStatusSignId",
                table: "Materials",
                column: "ProcessedStatusSignId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials",
                column: "ProcessedStatusSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: new Guid("c85a76f4-3c04-46f7-aed9-f865243b058e"));

            migrationBuilder.DeleteData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: new Guid("0a641312-abb7-4b40-a766-0781308eb077"));

            migrationBuilder.DeleteData(
                table: "MaterialSignTypes",
                keyColumn: "Id",
                keyValue: new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"));

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ProcessedStatusSignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ProcessedStatusSignId",
                table: "Materials");

            migrationBuilder.AddColumn<bool>(
                name: "IsProcessed",
                table: "Materials",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
