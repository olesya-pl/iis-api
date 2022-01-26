using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Iis.MaterialDistributor.DataModel.Migrations
{
    public partial class AddVariableCoefficientEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VariableCoefficients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    OffsetHours = table.Column<int>(nullable: false),
                    Coefficient = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VariableCoefficients", x => x.Id);
                });

            var columns = new[] { "Id", "OffsetHours", "Coefficient" };

            migrationBuilder.InsertData("VariableCoefficients", columns, new object[] { "382dc76d0aee4b16ab8daf86ab543ede", 0, 100 });
            migrationBuilder.InsertData("VariableCoefficients", columns, new object[] { "85eb04d32df04ad38aef1db82d6bb1eb", 1, 75 });
            migrationBuilder.InsertData("VariableCoefficients", columns, new object[] { "c5ff2326c4914d55b4d869be7daed13a", 2, 50 });
            migrationBuilder.InsertData("VariableCoefficients", columns, new object[] { "6df32f11314c4468bbc89b691cafd8b7", 3, 30 });
            migrationBuilder.InsertData("VariableCoefficients", columns, new object[] { "dc1ba907f24f40fbb613a0400c8dd884", 4, 10 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VariableCoefficients");
        }
    }
}
