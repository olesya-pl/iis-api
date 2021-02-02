using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class FixMaterialSignTypo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update public.\"MaterialSigns\" set \"Title\" = 'Здебільшого надійне' where \"Id\" = '521ad86b-af5d-4731-b5e7-e3e69ef23fc7'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
