using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsContstraintsRm2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaterialFeatures_NodeId",
                table: "MaterialFeatures");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialFeatures_NodeId",
                table: "MaterialFeatures",
                column: "NodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MaterialFeatures_NodeId",
                table: "MaterialFeatures");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialFeatures_NodeId",
                table: "MaterialFeatures",
                column: "NodeId",
                unique: true);
        }
    }
}
