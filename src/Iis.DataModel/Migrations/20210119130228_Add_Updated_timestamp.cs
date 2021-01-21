using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Add_Updated_timestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Themes",
                nullable: true);

            migrationBuilder.Sql("update public.\"Themes\" set \"UpdatedAt\" = timezone('UTC', now())");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Themes",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Themes");
        }
    }
}
