using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Iis.FlightRadar.DataModel.Migrations
{
    public partial class init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:enum_Aircraft_purposeType", "civil,millitary");

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    icao = table.Column<string>(maxLength: 255, nullable: true),
                    iata = table.Column<string>(maxLength: 255, nullable: true),
                    name = table.Column<string>(maxLength: 255, nullable: true),
                    country = table.Column<string>(maxLength: 255, nullable: true),
                    countryCode = table.Column<string>(maxLength: 255, nullable: true),
                    countryCodeLong = table.Column<string>(maxLength: 255, nullable: true),
                    city = table.Column<string>(maxLength: 255, nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    altitude = table.Column<decimal>(type: "numeric", nullable: true),
                    website = table.Column<string>(maxLength: 255, nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    icao = table.Column<string>(maxLength: 255, nullable: true),
                    iata = table.Column<string>(maxLength: 255, nullable: true),
                    name = table.Column<string>(maxLength: 255, nullable: true),
                    shortName = table.Column<string>(maxLength: 255, nullable: true),
                    country = table.Column<string>(maxLength: 255, nullable: true),
                    about = table.Column<string>(type: "json", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Aircraft",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    registration_number = table.Column<string>(maxLength: 255, nullable: true),
                    icao = table.Column<string>(maxLength: 255, nullable: true),
                    model = table.Column<string>(maxLength: 255, nullable: true),
                    detailedModel = table.Column<string>(maxLength: 255, nullable: true),
                    photo = table.Column<string>(maxLength: 255, nullable: true),
                    type = table.Column<string>(maxLength: 255, nullable: true),
                    ownerId = table.Column<int>(nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircraft", x => x.id);
                    table.ForeignKey(
                        name: "Aircraft_ownerId_fkey",
                        column: x => x.ownerId,
                        principalTable: "Operators",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    flightNo = table.Column<string>(fixedLength: true, maxLength: 100, nullable: true),
                    scheduledDepartureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    scheduledArrivalAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    realDepartureAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    realArrivalAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    externalId = table.Column<string>(maxLength: 100, nullable: true),
                    meta = table.Column<string>(type: "json", nullable: true),
                    arrivalAirportId = table.Column<int>(nullable: true),
                    departureAirportId = table.Column<int>(nullable: true),
                    planeId = table.Column<int>(nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.id);
                    table.ForeignKey(
                        name: "Flights_arrivalAirportId_fkey",
                        column: x => x.arrivalAirportId,
                        principalTable: "Airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Flights_departureAirportId_fkey",
                        column: x => x.departureAirportId,
                        principalTable: "Airports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "Flights_planeId_fkey",
                        column: x => x.planeId,
                        principalTable: "Aircraft",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Routes",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    callsign = table.Column<string>(maxLength: 255, nullable: true),
                    latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    altitude = table.Column<decimal>(type: "numeric", nullable: true),
                    track = table.Column<int>(nullable: true),
                    speed = table.Column<double>(nullable: true),
                    timeNow = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    squawk_code = table.Column<string>(fixedLength: true, maxLength: 100, nullable: true),
                    flightId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Routes", x => x.id);
                    table.ForeignKey(
                        name: "Routes_flightId_fkey",
                        column: x => x.flightId,
                        principalTable: "Flights",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Aircraft_ownerId",
                table: "Aircraft",
                column: "ownerId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_arrivalAirportId",
                table: "Flights",
                column: "arrivalAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_departureAirportId",
                table: "Flights",
                column: "departureAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_planeId",
                table: "Flights",
                column: "planeId");

            migrationBuilder.CreateIndex(
                name: "routes_flight_id_latitude_longitude_altitude_time_now",
                table: "Routes",
                columns: new[] { "flightId", "latitude", "longitude", "altitude", "timeNow" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Routes");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Airports");

            migrationBuilder.DropTable(
                name: "Aircraft");

            migrationBuilder.DropTable(
                name: "Operators");
        }
    }
}
