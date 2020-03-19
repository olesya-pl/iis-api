using System;
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
                    { new Guid("1107a504-c2a7-4f8b-a218-e5bbf5f281c4"), new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 1, "1", "Перша категорія" },
                    { new Guid("5406768c-581d-4b95-a549-b2cd1d09cfd8"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 4, "D", "Не завжди надійне" },
                    { new Guid("5342ead6-d478-4abc-b8d1-fd5d6a741706"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 3, "C", "Відносно надійне" },
                    { new Guid("521ad86b-af5d-4731-b5e7-e3e69ef23fc7"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 2, "B", "Здебільшего надійне" },
                    { new Guid("513de8b4-5c99-414f-94f1-513a716fc01c"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 1, "A", "Повністю надійне" },
                    { new Guid("44ddf35a-eeee-4aa3-9f3c-9b73dc1d63ee"), new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 4, "НРІ", "Недостатня розвідувальна інформація" },
                    { new Guid("431a888f-406b-458a-9905-abc752710659"), new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 3, "Ч", "Часткова" },
                    { new Guid("422914a7-f761-4075-a91e-4d34d33aedff"), new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 2, "ДП", "Достатньо повна" },
                    { new Guid("4124de78-0877-40b4-834a-f892060ea3f5"), new Guid("4061d06f-c14a-454e-9247-ccdd6d9388f0"), 1, "П", "Повна" },
                    { new Guid("354436fe-8c25-4352-8b89-3e94bf5828e2"), new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 5, "НI", "Неактуальна інформація" },
                    { new Guid("341892c9-3918-4a7f-bf61-d5b9050de7f4"), new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 4, "П", "Посередня" },
                    { new Guid("3317a961-1929-4957-9ef0-08b3007648a6"), new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 3, "А", "Актуальна" },
                    { new Guid("320c2a19-ed1b-4250-bb02-eb4f7391165b"), new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 2, "ДА", "Дуже актуальна" },
                    { new Guid("313d1f5b-7b3a-446f-ab92-e4046930a599"), new Guid("30106ade-e768-438a-b736-5c19df3ffd52"), 1, "У", "Упереджувальна" },
                    { new Guid("2616aa7d-c379-452b-8c1a-c815f9b989bc"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 6, "ДІ", "Дезінформація" },
                    { new Guid("25007914-d5cb-4def-8162-c12b4aa7038c"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 5, "НД", "Невизначеної достовірності" },
                    { new Guid("2475d991-b09e-4997-9d0a-2fc0bf07b1eb"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 4, "Н", "Недостовірна" },
                    { new Guid("2326d6ef-5542-42a8-83eb-0c2b92d188f1"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 3, "С", "Сумнівна" },
                    { new Guid("225f189b-9ad2-4687-9624-0d4c991a3d6b"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 2, "Й", "Ймовірна" },
                    { new Guid("211f5765-0867-4d04-976a-70f3e34bf153"), new Guid("202f605f-4fb2-49a7-beb8-e40cd41f2b83"), 1, "Д", "Достовірна" },
                    { new Guid("1356a6b3-c63f-4985-8b74-372236fe744f"), new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 3, "3", "Третя категорія" },
                    { new Guid("1240c504-8ecd-4aca-9b75-24f0c6304426"), new Guid("10170a81-2916-420b-8bd7-5688cb43b82f"), 2, "2", "Друга категорія" },
                    { new Guid("55b0a038-2347-4fb0-82e1-6081933ac9e1"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 5, "E", "Ненадійне" },
                    { new Guid("56365559-24fb-42f2-8305-bbef01fd6e3e"), new Guid("5023bb79-f987-48fe-a86c-38b7aa8495c4"), 6, "F", "Неможливо оцінити надійність" }
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
