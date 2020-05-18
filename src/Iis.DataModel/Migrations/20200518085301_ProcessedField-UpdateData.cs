using Microsoft.EntityFrameworkCore.Migrations;
using System;
namespace IIS.Core.Migrations
{
    public partial class ProcessedFieldUpdateData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaterialSignTypes",
                keyColumn: "Id",
                keyValue: new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"),
                column: "Name",
                value: "ProcessedStatus");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaterialSignTypes",
                keyColumn: "Id",
                keyValue: new Guid("214ceeee-67d5-4692-a3b4-316007fa5d34"),
                column: "Name",
                value: "ProcessingStatus");
        }
    }
}
