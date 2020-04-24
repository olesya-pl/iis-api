using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json.Linq;

namespace IIS.Core.Migrations
{
    public partial class MaterialInfo_Data_NotNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"update public.\"MaterialInfos\" set \"Data\"='{new JObject()}' " +
                $"where \"Data\" is null");

            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "MaterialInfos",
                nullable: false,
                defaultValue: "{}",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Data",
                table: "MaterialInfos",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldDefaultValue: "{}");
        }
    }
}
