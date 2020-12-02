using System;
using Iis.DataModel.Materials;
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

            migrationBuilder.InsertData(
                table: "MaterialSignTypes",
                columns: new[] { "Id", "Name", "Title" },
                values: new object[,]
                {
                    { new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), "SessionPriority", "Прiоритет сеансу" }
                });

            migrationBuilder.InsertData(
                table: "MaterialSigns",
                columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { MaterialEntity.SessionPriorityImportantSignId, new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 1, "В", "Важливий" },
                    { MaterialEntity.SessionPriorityImmediateReportSignId, new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 2, "Н", "Негайна доповідь" },
                    { MaterialEntity.SessionPrioritySkipSignId, new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 3, "П", "Пропустити" },
                    { MaterialEntity.SessionPriorityTranslateSignId, new Guid("6037bd7a-97df-4fa2-9c94-20468453a534"), 4, "ПЕР", "Переклад" }
                });
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
