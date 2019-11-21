using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AnalyticsQueryAddRootIndicatorField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RootIndicatorId",
                table: "AnalyticsQuery",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQuery_RootIndicatorId",
                table: "AnalyticsQuery",
                column: "RootIndicatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AnalyticsQuery_AnalyticsIndicators_RootIndicatorId",
                table: "AnalyticsQuery",
                column: "RootIndicatorId",
                principalTable: "AnalyticsIndicators",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AnalyticsQuery_AnalyticsIndicators_RootIndicatorId",
                table: "AnalyticsQuery");

            migrationBuilder.DropIndex(
                name: "IX_AnalyticsQuery_RootIndicatorId",
                table: "AnalyticsQuery");

            migrationBuilder.DropColumn(
                name: "RootIndicatorId",
                table: "AnalyticsQuery");
        }
    }
}
