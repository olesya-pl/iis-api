using Microsoft.EntityFrameworkCore.Migrations;

namespace Iis.FlightRadar.DataModel.Migrations
{
    public partial class ClearDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
TRUNCATE TABLE public.""Aircraft"" CASCADE;
TRUNCATE TABLE public.""Airports"" CASCADE;
TRUNCATE TABLE public.""Flights"" CASCADE;
TRUNCATE TABLE public.""Operators"" CASCADE;
TRUNCATE TABLE public.""Routes"" CASCADE;");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}