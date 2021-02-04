using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MakeTowerLocationKeyless : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TowerLocations",
                table: "TowerLocations");

            migrationBuilder.DropIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lac_CellId",
                table: "TowerLocations");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddPrimaryKey(
                name: "PK_TowerLocations",
                table: "TowerLocations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lac_CellId",
                table: "TowerLocations",
                columns: new[] { "Mcc", "Mnc", "Lac", "CellId" })
                .Annotation("Npgsql:IndexInclude", new[] { "Lat", "Long" });
        }
    }
}
