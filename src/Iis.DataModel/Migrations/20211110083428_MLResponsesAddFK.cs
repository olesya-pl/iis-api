using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MLResponsesAddFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM public.\"MLResponses\" WHERE \"Id\" IN(select f.\"Id\" from public.\"MLResponses\" f LEFT JOIN public.\"Materials\" d on f.\"MaterialId\" = d.\"Id\" WHERE d.\"Id\" IS NULL);");

            migrationBuilder.CreateIndex(
                name: "IX_MLResponses_MaterialId",
                table: "MLResponses",
                column: "MaterialId");

            migrationBuilder.AddForeignKey(
                name: "FK_MLResponses_Materials_MaterialId",
                table: "MLResponses",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MLResponses_Materials_MaterialId",
                table: "MLResponses");

            migrationBuilder.DropIndex(
                name: "IX_MLResponses_MaterialId",
                table: "MLResponses");
        }
    }
}
