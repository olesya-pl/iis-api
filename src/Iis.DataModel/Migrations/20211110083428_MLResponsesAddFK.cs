using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MLResponsesAddFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from public.\"MLResponses\" where \"MaterialId\" not in (select \"Id\" from public.\"Materials\")");

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
