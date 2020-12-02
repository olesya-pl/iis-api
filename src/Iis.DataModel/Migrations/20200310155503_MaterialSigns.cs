using System;
using Iis.DataModel.Materials;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class MaterialSigns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CompletenessSignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImportanceSignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelevanceSignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReliabilitySignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceReliabilitySignId",
                table: "Materials",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MaterialSignTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSignTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialSigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MaterialSignTypeId = table.Column<Guid>(nullable: false),
                    ShortTitle = table.Column<string>(nullable: true),
                    Title = table.Column<string>(nullable: true),
                    OrderNumber = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialSigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialSigns_MaterialSignTypes_MaterialSignTypeId",
                        column: x => x.MaterialSignTypeId,
                        principalTable: "MaterialSignTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "MaterialSignTypes",
                columns: new[] { "Id", "Name", "Title" },
                values: new object[,]
                {
                    { new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), "Importance", "Важливість" },
                    { new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), "Reliability", "Достовірність" },
                    { new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), "Relevance", "Актуальність" },
                    { new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), "Completeness", "Повнота" },
                    { new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), "SourceReliability", "Надійність джерела" }
                });

            migrationBuilder.InsertData(
                table: "MaterialSigns",
                columns: new[] { "Id", "MaterialSignTypeId", "OrderNumber", "ShortTitle", "Title" },
                values: new object[,]
                {
                    { MaterialEntity.ImportanceFirstCategorySignId, new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 1, "1", "Перша категорія" },
                    { MaterialEntity.SourceReliabilityRarelyReliableSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 4, "D", "Не завжди надійне" },
                    { MaterialEntity.SourceReliabilityRelativelyReliableSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 3, "C", "Відносно надійне" },
                    { MaterialEntity.SourceReliabilityMostlyReliableSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 2, "B", "Здебільшего надійне" },
                    { MaterialEntity.SourceReliabilityCompletelyReliableSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 1, "A", "Повністю надійне" },
                    { MaterialEntity.CompletenessNotEnoughDataSignId, new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 4, "НРІ", "Недостатня розвідувальна інформація" },
                    { MaterialEntity.CompletenessPartialSignId, new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 3, "Ч", "Часткова" },
                    { MaterialEntity.CompletenessCompleteEnoughSignId, new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 2, "ДП", "Достатньо повна" },
                    { MaterialEntity.CompletenessCompleteSignId, new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 1, "П", "Повна" },
                    { MaterialEntity.RelevanceIrrelevantSignId, new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 5, "НI", "Неактуальна інформація" },
                    { MaterialEntity.RelevanceAverageRelevanceSignId, new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 4, "П", "Посередня" },
                    { MaterialEntity.RelevanceRelevantSignId, new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 3, "А", "Актуальна" },
                    { MaterialEntity.RelevanceVeryRelevantSignId, new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 2, "ДА", "Дуже актуальна" },
                    { MaterialEntity.RelevanceWarningSignId, new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 1, "У", "Упереджувальна" },
                    { MaterialEntity.ReliabilityDesinformationSignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 6, "ДІ", "Дезінформація" },
                    { MaterialEntity.ReliabilityUnknownReliabilitySignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 5, "НД", "Невизначеної достовірності" },
                    { MaterialEntity.ReliabilityUnreliableSignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 4, "Н", "Недостовірна" },
                    { MaterialEntity.ReliabilityDoubtfulSignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 3, "С", "Сумнівна" },
                    { MaterialEntity.ReliabilityProbableSignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 2, "Й", "Ймовірна" },
                    { MaterialEntity.ReliabilityReliableSignId, new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 1, "Д", "Достовірна" },
                    { MaterialEntity.ImportanceThirdCategorySignId, new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 3, "3", "Третя категорія" },
                    { MaterialEntity.ImportanceSecondCategorySignId, new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 2, "2", "Друга категорія" },
                    { MaterialEntity.SourceReliabilityUnreliableSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 5, "E", "Ненадійне" },
                    { MaterialEntity.SourceReliabilityCannotEstimateSignId, new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 6, "F", "Неможливо оцінити надійність" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Materials_CompletenessSignId",
                table: "Materials",
                column: "CompletenessSignId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ImportanceSignId",
                table: "Materials",
                column: "ImportanceSignId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_RelevanceSignId",
                table: "Materials",
                column: "RelevanceSignId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_ReliabilitySignId",
                table: "Materials",
                column: "ReliabilitySignId");

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SourceReliabilitySignId",
                table: "Materials",
                column: "SourceReliabilitySignId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialSigns_MaterialSignTypeId",
                table: "MaterialSigns",
                column: "MaterialSignTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_CompletenessSignId",
                table: "Materials",
                column: "CompletenessSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_ImportanceSignId",
                table: "Materials",
                column: "ImportanceSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_RelevanceSignId",
                table: "Materials",
                column: "RelevanceSignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_ReliabilitySignId",
                table: "Materials",
                column: "ReliabilitySignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_MaterialSigns_SourceReliabilitySignId",
                table: "Materials",
                column: "SourceReliabilitySignId",
                principalTable: "MaterialSigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_CompletenessSignId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_ImportanceSignId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_RelevanceSignId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_ReliabilitySignId",
                table: "Materials");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_MaterialSigns_SourceReliabilitySignId",
                table: "Materials");

            migrationBuilder.DropTable(
                name: "MaterialSigns");

            migrationBuilder.DropTable(
                name: "MaterialSignTypes");

            migrationBuilder.DropIndex(
                name: "IX_Materials_CompletenessSignId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ImportanceSignId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_RelevanceSignId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_ReliabilitySignId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SourceReliabilitySignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "CompletenessSignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ImportanceSignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "RelevanceSignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "ReliabilitySignId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SourceReliabilitySignId",
                table: "Materials");
        }
    }
}
