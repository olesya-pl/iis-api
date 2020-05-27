using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsUseUnixMindate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update public.""Materials"" set ""CreatedDate"" = '1970-01-01 00:00:00'
where ""CreatedDate"" = '0001-01-01 00:00:00'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
