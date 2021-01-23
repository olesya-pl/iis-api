using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddedTowerLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistory_Nodes_EntityId",
                table: "LocationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistory_Nodes_NodeId",
                table: "LocationHistory");

            migrationBuilder.AlterColumn<Guid>(
                name: "NodeId",
                table: "LocationHistory",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "EntityId",
                table: "LocationHistory",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "MaterialId",
                table: "LocationHistory",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "LocationHistory",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TowerLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Mcc = table.Column<string>(nullable: true),
                    Mnc = table.Column<string>(nullable: true),
                    Lac = table.Column<string>(nullable: true),
                    CellId = table.Column<string>(nullable: true),
                    Lat = table.Column<decimal>(nullable: false),
                    Long = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TowerLocations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationHistory_MaterialId",
                table: "LocationHistory",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TowerLocations_Mcc_Mnc_Lat_CellId",
                table: "TowerLocations",
                columns: new[] { "Mcc", "Mnc", "Lat", "CellId" },
                unique: true)
                .Annotation("Npgsql:IndexInclude", new[] { "Lac", "Long" });

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistory_Nodes_EntityId",
                table: "LocationHistory",
                column: "EntityId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistory_Materials_MaterialId",
                table: "LocationHistory",
                column: "MaterialId",
                principalTable: "Materials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistory_Nodes_NodeId",
                table: "LocationHistory",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistory_Nodes_EntityId",
                table: "LocationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistory_Materials_MaterialId",
                table: "LocationHistory");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationHistory_Nodes_NodeId",
                table: "LocationHistory");

            migrationBuilder.DropTable(
                name: "TowerLocations");

            migrationBuilder.DropIndex(
                name: "IX_LocationHistory_MaterialId",
                table: "LocationHistory");

            migrationBuilder.DropColumn(
                name: "MaterialId",
                table: "LocationHistory");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "LocationHistory");

            migrationBuilder.AlterColumn<Guid>(
                name: "NodeId",
                table: "LocationHistory",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EntityId",
                table: "LocationHistory",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistory_Nodes_EntityId",
                table: "LocationHistory",
                column: "EntityId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationHistory_Nodes_NodeId",
                table: "LocationHistory",
                column: "NodeId",
                principalTable: "Nodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
