using Iis.MaterialDistributor.DataModel.Contexts;
using Iis.MaterialDistributor.DataModel.Entities;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Iis.MaterialDistributor.DataModel.Migrations
{
    public partial class SeedPermanentCoefficients : Migration
    {
        private const string TableName = nameof(MaterialDistributorContext.PermanentCoefficients);
        private const string NameColumn = nameof(PermanentCoefficientEntity.Name);
        private const string ValueColumn = nameof(PermanentCoefficientEntity.Value);

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermanentCriterias");

            migrationBuilder.CreateTable(
                name: "PermanentCoefficients",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    Value = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermanentCoefficients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermanentCoefficients_Name",
                table: "PermanentCoefficients",
                column: "Name",
                unique: true);

            var columns = new[]
            {
                NameColumn,
                ValueColumn
            };

            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.RelatedToObjectOfStudy, PermanentCoefficientEntity.RelatedToObjectOfStudyValue });
            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.HasPhoneNumber, PermanentCoefficientEntity.HasPhoneNumberValue });
            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.HasIridiumOptions, PermanentCoefficientEntity.HasIridiumOptionsValue });
            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.HasTMSI, PermanentCoefficientEntity.HasTMSIValue });
            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.RelatedWithHighPriority, PermanentCoefficientEntity.RelatedWithHighPriorityValue });
            migrationBuilder.InsertData(TableName, columns, new object[] { PermanentCoefficientEntity.RelatedAndIgnoredPriority, PermanentCoefficientEntity.RelatedAndIgnoredPriorityValue });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermanentCoefficients");

            migrationBuilder.CreateTable(
                name: "PermanentCriterias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermanentCriterias", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermanentCriterias_Name",
                table: "PermanentCriterias",
                column: "Name",
                unique: true);

            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.RelatedToObjectOfStudy);
            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.HasPhoneNumber);
            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.HasIridiumOptions);
            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.HasTMSI);
            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.RelatedWithHighPriority);
            migrationBuilder.DeleteData(TableName, NameColumn, PermanentCoefficientEntity.RelatedAndIgnoredPriority);
        }
    }
}