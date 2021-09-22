using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IIS.Core.Migrations
{
    public partial class Add_Many_Assignee_To_MaterialEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MaterialAssignees",
                columns: table => new
                {
                    MaterialId = table.Column<Guid>(nullable: false),
                    AssigneeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialAssignees", x => new { x.MaterialId, x.AssigneeId });
                    table.ForeignKey(
                        name: "FK_MaterialAssignees_Users_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialAssignees_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MaterialAssignees_AssigneeId",
                table: "MaterialAssignees",
                column: "AssigneeId");

            migrationBuilder.Sql(@"INSERT INTO public.""MaterialAssignees""(""MaterialId"", ""AssigneeId"")
SELECT materials.""Id"" AS ""MaterialId"", materials.""AssigneeId""
FROM public.""Materials"" AS materials
WHERE materials.""AssigneeId"" IS NOT NULL
ON CONFLICT ON CONSTRAINT ""PK_MaterialAssignees""
DO NOTHING");

            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Users_AssigneeId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_AssigneeId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "AssigneeId",
                table: "Materials");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MaterialAssignees");

            migrationBuilder.AddColumn<Guid>(
                name: "AssigneeId",
                table: "Materials",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_AssigneeId",
                table: "Materials",
                column: "AssigneeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Users_AssigneeId",
                table: "Materials",
                column: "AssigneeId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
