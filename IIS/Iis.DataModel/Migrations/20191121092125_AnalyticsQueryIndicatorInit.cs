using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class AnalyticsQueryIndicatorInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnalyticsQueryIndicators",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    QueryId = table.Column<Guid>(nullable: false),
                    IndicatorId = table.Column<Guid>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalyticsQueryIndicators", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalyticsQueryIndicators_AnalyticsIndicators_IndicatorId",
                        column: x => x.IndicatorId,
                        principalTable: "AnalyticsIndicators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AnalyticsQueryIndicators_AnalyticsQuery_QueryId",
                        column: x => x.QueryId,
                        principalTable: "AnalyticsQuery",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQueryIndicators_IndicatorId",
                table: "AnalyticsQueryIndicators",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQueryIndicators_QueryId",
                table: "AnalyticsQueryIndicators",
                column: "QueryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalyticsQueryIndicators");
        }
    }
}
