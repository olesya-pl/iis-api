using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialSessionPriority : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImportantSession",
                table: "Materials");

            migrationBuilder.AddColumn<Guid>(
                name: "SessionPriorityId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SessionPriorityId",
                table: "Materials",
                column: "SessionPriorityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_SessionPriorityId",
                table: "Materials",
                column: "SessionPriorityId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            //migrationBuilder.InsertData(
            //    table: "MaterialSignTypes",
            //    columns: new[] { "Id", "Name", "Title" },
            //    values: new object[,]
            //    {
            //        { new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), "SessionPriority", "Прiоритет сеансу" }
            //    });

            //migrationBuilder.InsertData(
            //    table: "MaterialSigns",
            //    columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
            //    values: new object[,]
            //    {
            //        { new Guid("6051013d-846b-4409-9da0-9414b103b396"), new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 1, "В", "Важливий" },
            //        { new Guid("60536b3c-e78d-40cf-9199-37f112184f69"), new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 2, "Н", "Негайна доповідь" },
            //        { new Guid("60560b14-195c-4605-816e-983118ab9ed9"), new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 3, "П", "Пропустити" },
            //        { new Guid("6071f9f3-1988-4b14-9f6a-c0514fc795d0"), new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 4, "ПЕР", "Переклад" }
            //    });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_SessionPriorityId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SessionPriorityId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SessionPriorityId",
                table: "Materials");

            migrationBuilder.AddColumn<bool>(
                name: "IsImportantSession",
                table: "Materials",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
