using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Refactored : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeTypes_Types_Id",
                table: "AttributeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_Types_TypeId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_Types_Id",
                table: "RelationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_Types_SourceTypeId",
                table: "RelationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_Types_TargetTypeId",
                table: "RelationTypes");

            //migrationBuilder.DropTable(
            //    name: "AnalyticsQueryIndicators");

            //migrationBuilder.DropTable(
            //    name: "Types");

            //migrationBuilder.DropTable(
            //    name: "AnalyticsIndicators");

            //migrationBuilder.DropTable(
            //    name: "AnalyticsQuery");

            //migrationBuilder.DropIndex(
            //    name: "IX_Users_Username",
            //    table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Materials_FileId",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "TypeId",
                table: "Nodes",
                newName: "NodeTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Nodes_TypeId",
                table: "Nodes",
                newName: "IX_Nodes_NodeTypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users",
                column: "Username");

            migrationBuilder.RenameTable("AnalyticsIndicators", newName: "AnalyticIndicators");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticIndicators",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        Title = table.Column<string>(maxLength: 200, nullable: false),
            //        Query = table.Column<string>(nullable: true),
            //        ParentId = table.Column<Guid>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticIndicators", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticIndicators_AnalyticIndicators_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "AnalyticIndicators",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            migrationBuilder.RenameTable("AnalyticsQuery", newName: "AnalyticQueries");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticQueries",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        Title = table.Column<string>(maxLength: 500, nullable: false),
            //        Description = table.Column<string>(maxLength: 1000, nullable: true),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        UpdatedAt = table.Column<DateTime>(nullable: false),
            //        CreatorId = table.Column<Guid>(nullable: false),
            //        LastUpdaterId = table.Column<Guid>(nullable: false),
            //        DateRanges = table.Column<string>(type: "jsonb", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticQueries", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticQueries_Users_CreatorId",
            //            column: x => x.CreatorId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AnalyticQueries_Users_LastUpdaterId",
            //            column: x => x.LastUpdaterId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.RenameTable("Types", newName: "NodeTypes");
            //migrationBuilder.CreateTable(
            //    name: "NodeTypes",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        Name = table.Column<string>(nullable: true),
            //        Title = table.Column<string>(nullable: true),
            //        Meta = table.Column<string>(nullable: true),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        UpdatedAt = table.Column<DateTime>(nullable: false),
            //        IsArchived = table.Column<bool>(nullable: false),
            //        Kind = table.Column<int>(nullable: false),
            //        IsAbstract = table.Column<bool>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_NodeTypes", x => x.Id);
            //    });

            migrationBuilder.RenameTable("AnalyticsQueryIndicators", newName: "AnalyticQueryIndicators");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticQueryIndicators",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        QueryId = table.Column<Guid>(nullable: false),
            //        IndicatorId = table.Column<Guid>(nullable: false),
            //        Title = table.Column<string>(nullable: true),
            //        SortOrder = table.Column<int>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticQueryIndicators", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticQueryIndicators_AnalyticIndicators_IndicatorId",
            //            column: x => x.IndicatorId,
            //            principalTable: "AnalyticIndicators",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AnalyticQueryIndicators_AnalyticQueries_QueryId",
            //            column: x => x.QueryId,
            //            principalTable: "AnalyticQueries",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_FileId",
                table: "Materials",
                column: "FileId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticIndicators_ParentId",
                table: "AnalyticIndicators",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticQueries_CreatorId",
                table: "AnalyticQueries",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticQueries_LastUpdaterId",
                table: "AnalyticQueries",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticQueryIndicators_IndicatorId",
                table: "AnalyticQueryIndicators",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticQueryIndicators_QueryId",
                table: "AnalyticQueryIndicators",
                column: "QueryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeTypes_NodeTypes_Id",
                table: "AttributeTypes",
                column: "Id",
                principalTable: "NodeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_NodeTypes_NodeTypeId",
                table: "Nodes",
                column: "NodeTypeId",
                principalTable: "NodeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_NodeTypes_Id",
                table: "RelationTypes",
                column: "Id",
                principalTable: "NodeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_NodeTypes_SourceTypeId",
                table: "RelationTypes",
                column: "SourceTypeId",
                principalTable: "NodeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_NodeTypes_TargetTypeId",
                table: "RelationTypes",
                column: "TargetTypeId",
                principalTable: "NodeTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttributeTypes_NodeTypes_Id",
                table: "AttributeTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_NodeTypes_NodeTypeId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_NodeTypes_Id",
                table: "RelationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_NodeTypes_SourceTypeId",
                table: "RelationTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_RelationTypes_NodeTypes_TargetTypeId",
                table: "RelationTypes");

            //migrationBuilder.DropTable(
            //    name: "AnalyticQueryIndicators");

            //migrationBuilder.DropTable(
            //    name: "NodeTypes");

            //migrationBuilder.DropTable(
            //    name: "AnalyticIndicators");

            //migrationBuilder.DropTable(
            //    name: "AnalyticQueries");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Username",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Materials_FileId",
                table: "Materials");

            migrationBuilder.RenameColumn(
                name: "NodeTypeId",
                table: "Nodes",
                newName: "TypeId");

            migrationBuilder.RenameIndex(
                name: "IX_Nodes_NodeTypeId",
                table: "Nodes",
                newName: "IX_Nodes_TypeId");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.RenameTable("AnalyticIndicators", newName: "AnalyticsIndicators");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticsIndicators",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        ParentId = table.Column<Guid>(nullable: true),
            //        Query = table.Column<string>(nullable: true),
            //        Title = table.Column<string>(maxLength: 200, nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticsIndicators", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticsIndicators_AnalyticsIndicators_ParentId",
            //            column: x => x.ParentId,
            //            principalTable: "AnalyticsIndicators",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Restrict);
            //    });

            migrationBuilder.RenameTable("AnalyticQueries", newName: "AnalyticsQuery");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticsQuery",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        CreatorId = table.Column<Guid>(nullable: false),
            //        DateRanges = table.Column<string>(type: "jsonb", nullable: true),
            //        Description = table.Column<string>(maxLength: 1000, nullable: true),
            //        LastUpdaterId = table.Column<Guid>(nullable: false),
            //        Title = table.Column<string>(maxLength: 500, nullable: false),
            //        UpdatedAt = table.Column<DateTime>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticsQuery", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticsQuery_Users_CreatorId",
            //            column: x => x.CreatorId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AnalyticsQuery_Users_LastUpdaterId",
            //            column: x => x.LastUpdaterId,
            //            principalTable: "Users",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.RenameTable("NodeTypes", newName: "Types");
            //migrationBuilder.CreateTable(
            //    name: "Types",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        CreatedAt = table.Column<DateTime>(nullable: false),
            //        IsAbstract = table.Column<bool>(nullable: false),
            //        IsArchived = table.Column<bool>(nullable: false),
            //        Kind = table.Column<int>(nullable: false),
            //        Meta = table.Column<string>(nullable: true),
            //        Name = table.Column<string>(nullable: true),
            //        Title = table.Column<string>(nullable: true),
            //        UpdatedAt = table.Column<DateTime>(nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_Types", x => x.Id);
            //    });

            migrationBuilder.RenameTable("AnalyticQueryIndicators", newName: "AnalyticsQueryIndicators");
            //migrationBuilder.CreateTable(
            //    name: "AnalyticsQueryIndicators",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(nullable: false),
            //        IndicatorId = table.Column<Guid>(nullable: false),
            //        QueryId = table.Column<Guid>(nullable: false),
            //        SortOrder = table.Column<int>(nullable: false),
            //        Title = table.Column<string>(nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AnalyticsQueryIndicators", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AnalyticsQueryIndicators_AnalyticsIndicators_IndicatorId",
            //            column: x => x.IndicatorId,
            //            principalTable: "AnalyticsIndicators",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //        table.ForeignKey(
            //            name: "FK_AnalyticsQueryIndicators_AnalyticsQuery_QueryId",
            //            column: x => x.QueryId,
            //            principalTable: "AnalyticsQuery",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_FileId",
                table: "Materials",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsIndicators_ParentId",
                table: "AnalyticsIndicators",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQuery_CreatorId",
                table: "AnalyticsQuery",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQuery_LastUpdaterId",
                table: "AnalyticsQuery",
                column: "LastUpdaterId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQueryIndicators_IndicatorId",
                table: "AnalyticsQueryIndicators",
                column: "IndicatorId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalyticsQueryIndicators_QueryId",
                table: "AnalyticsQueryIndicators",
                column: "QueryId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttributeTypes_Types_Id",
                table: "AttributeTypes",
                column: "Id",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_Types_TypeId",
                table: "Nodes",
                column: "TypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_Types_Id",
                table: "RelationTypes",
                column: "Id",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_Types_SourceTypeId",
                table: "RelationTypes",
                column: "SourceTypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RelationTypes_Types_TargetTypeId",
                table: "RelationTypes",
                column: "TargetTypeId",
                principalTable: "Types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
