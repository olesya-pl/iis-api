using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MachineLearning_AddNewFields_Version_and_ProcessingDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HandlerName",
                table: "MLResponses",
                nullable: true);

            migrationBuilder.Sql("update public.\"MLResponses\" set \"HandlerName\" = \"MLHandlerName\"");

            migrationBuilder.AlterColumn<string>(
                name: "HandlerName",
                table: "MLResponses",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HandlerVersion",
                table: "MLResponses",
                nullable: true);

            migrationBuilder.Sql("update public.\"MLResponses\" set \"HandlerVersion\" = '1.0'");

            migrationBuilder.AlterColumn<string>(
                name: "HandlerVersion",
                table: "MLResponses",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);


            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessingDate",
                table: "MLResponses",
                nullable: true);

            migrationBuilder.Sql("update public.\"MLResponses\" set \"ProcessingDate\" = now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessingDate",
                table: "MLResponses",
                nullable: false,
                defaultValue: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HandlerName",
                table: "MLResponses");

            migrationBuilder.DropColumn(
                name: "HandlerVersion",
                table: "MLResponses");

            migrationBuilder.DropColumn(
                name: "ProcessingDate",
                table: "MLResponses");
        }
    }
}
