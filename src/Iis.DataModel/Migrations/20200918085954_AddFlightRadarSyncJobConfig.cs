using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AddFlightRadarSyncJobConfig : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightRadarHistorySyncJobConfig",
                columns: table => new
                {
                    LatestProcessedId = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightRadarHistorySyncJobConfig", x => x.LatestProcessedId);
                });

            migrationBuilder.Sql("INSERT INTO public.\"FlightRadarHistorySyncJobConfig\" (\"LatestProcessedId\") VALUES (0)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightRadarHistorySyncJobConfig");
        }
    }
}
