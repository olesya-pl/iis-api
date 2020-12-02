using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddMaterialProcessingStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: MaterialEntity.ProcessingStatusNotProcessedSignId,
                columns: new[] { "OrderNumber", "ShortTitle" },
                values: new object[] {
                    4, "4" 
                });

            migrationBuilder.InsertData(
                table: "MaterialSigns",
                columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { MaterialEntity.ProcessingStatusPrimaryProcessingSignId, MaterialEntity.ProcessingStatusSignTypeId, 3, "3", "Пройшов первинну обробку" },
                });

            migrationBuilder.InsertData(
                table: "MaterialSigns",
                columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { MaterialEntity.ProcessingStatusProcessingSignId, MaterialEntity.ProcessingStatusSignTypeId, 2, "2", "В обробці" },
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: MaterialEntity.ProcessingStatusProcessingSignId);

            migrationBuilder.DeleteData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: MaterialEntity.ProcessingStatusPrimaryProcessingSignId);

            migrationBuilder.UpdateData(
                table: "MaterialSigns",
                keyColumn: "Id",
                keyValue: MaterialEntity.ProcessingStatusNotProcessedSignId,
                columns: new[] { "OrderNumber", "ShortTitle" },
                values: new object[] {
                    2, "2"
                });
        }
    }
}
