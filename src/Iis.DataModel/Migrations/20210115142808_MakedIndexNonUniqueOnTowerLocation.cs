using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MakedIndexNonUniqueOnTowerLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lat_CellId",
                table: "TowerLocations");

            migrationBuilder.CreateIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lat_CellId",
                table: "TowerLocations",
                columns: new[] { "Mcc", "Mnc", "Lat", "CellId" })
                .Annotation("Npgsql:IndexInclude", new[] { "Lac", "Long" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lat_CellId",
                table: "TowerLocations");

            migrationBuilder.CreateIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lat_CellId",
                table: "TowerLocations",
                columns: new[] { "Mcc", "Mnc", "Lat", "CellId" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "Lac", "Long" });
        }
    }
}
