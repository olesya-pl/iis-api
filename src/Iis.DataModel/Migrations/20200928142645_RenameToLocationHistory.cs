using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class RenameToLocationHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE public.\"FlightRadarHistorySyncJobConfig\"");
            migrationBuilder.Sql("INSERT INTO public.\"FlightRadarHistorySyncJobConfig\" VALUES (0)");

            migrationBuilder.DropTable(
                name: "FlightRadarHistoryEntities");

            migrationBuilder.CreateTable(
                name: "LocationHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Lat = table.Column<decimal>(nullable: false),
                    Long = table.Column<decimal>(nullable: false),
                    RegisteredAt = table.Column<DateTime>(nullable: false),
                    NodeId = table.Column<Guid>(nullable: false),
                    EntityId = table.Column<Guid>(nullable: false),
                    ExternalId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationHistory_Nodes_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LocationHistory_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationHistory_EntityId",
                table: "LocationHistory",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationHistory_NodeId",
                table: "LocationHistory",
                column: "NodeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationHistory");

            migrationBuilder.CreateTable(
                name: "FlightRadarHistoryEntities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalId = table.Column<string>(type: "text", nullable: true),
                    ICAO = table.Column<string>(type: "text", nullable: true),
                    Lat = table.Column<decimal>(type: "numeric", nullable: false),
                    Long = table.Column<decimal>(type: "numeric", nullable: false),
                    NodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightRadarHistoryEntities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightRadarHistoryEntities_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightRadarHistoryEntities_NodeId",
                table: "FlightRadarHistoryEntities",
                column: "NodeId");
        }
    }
}
