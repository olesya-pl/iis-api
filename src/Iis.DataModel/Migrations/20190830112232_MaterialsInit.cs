using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialsInit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Materials",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ParentId = table.Column<Guid>(nullable: false),
                    FileId = table.Column<Guid>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Materials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Materials_Files_FileId",
                        column: x => x.FileId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Materials_Materials_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MaterialId = table.Column<Guid>(nullable: false),
                    Data = table.Column<string>(nullable: true),
                    Source = table.Column<string>(nullable: true),
                    SourceType = table.Column<string>(nullable: true),
                    SourceVersion = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialInfos_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MaterialFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MaterialInfoId = table.Column<Guid>(nullable: false),
                    Relation = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    NodeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialFeatures_MaterialInfos_MaterialInfoId",
                        column: x => x.MaterialInfoId,
                        principalTable: "MaterialInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialFeatures_Nodes_NodeId",
                        column: x => x.NodeId,
                        principalTable: "Nodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialFeatures_MaterialInfoId",
                table: "MaterialFeatures",
                column: "MaterialInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialFeatures_NodeId",
                table: "MaterialFeatures",
                column: "NodeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialInfos_MaterialId",
                table: "MaterialInfos",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_FileId",
                table: "Materials",
                column: "FileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ParentId",
                table: "Materials",
                column: "ParentId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialFeatures");

            migrationBuilder.DropTable(
                name: "MaterialInfos");

            migrationBuilder.DropTable(
                name: "Materials");
        }
    }
}
