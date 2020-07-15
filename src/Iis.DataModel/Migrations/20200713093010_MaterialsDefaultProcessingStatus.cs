using System;
using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsDefaultProcessingStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($@"update public.""Materials""
                                    set ""ProcessedStatusSignId"" = '{MaterialEntity.ProcessingStatusNotProcessedSignId}'
                                    where ""ProcessedStatusSignId"" is null");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcessedStatusSignId",
                table: "Materials",
                nullable: false,
                defaultValue: MaterialEntity.ProcessingStatusNotProcessedSignId,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials",
                column: "ProcessedStatusSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProcessedStatusSignId",
                table: "Materials",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldDefaultValue: MaterialEntity.ProcessingStatusNotProcessedSignId);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_ProcessedStatusSignId",
                table: "Materials",
                column: "ProcessedStatusSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
