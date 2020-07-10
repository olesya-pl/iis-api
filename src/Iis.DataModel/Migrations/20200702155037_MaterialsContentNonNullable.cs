using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsContentNonNullable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update public.\"Materials\" set \"Content\" = '' where \"Content\" is null");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Materials",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Materials",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldDefaultValue: "");
        }
    }
}
