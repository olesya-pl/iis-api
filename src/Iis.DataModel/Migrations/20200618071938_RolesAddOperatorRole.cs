using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class RolesAddOperatorRole : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
//            migrationBuilder.Sql("INSERT INTO public.\"Roles\" (\"Id\", \"Name\", \"IsAdmin\", \"IsArchived\") " +
//"VALUES('a120c2b8-d6f8-4338-ab0e-5d177951f119', 'Оператор', false, false)" +
//"ON CONFLICT DO NOTHING");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
