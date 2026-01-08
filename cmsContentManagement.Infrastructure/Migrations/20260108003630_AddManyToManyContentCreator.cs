using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmsContentManagment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManyToManyContentCreator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Creators_CreatorId",
                table: "Contents");

            migrationBuilder.DropIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Contents");

            migrationBuilder.CreateTable(
                name: "ContentCreator",
                columns: table => new
                {
                    ContentsContentId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    CreatorsCreatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentCreator", x => new { x.ContentsContentId, x.CreatorsCreatorId });
                    table.ForeignKey(
                        name: "FK_ContentCreator_Contents_ContentsContentId",
                        column: x => x.ContentsContentId,
                        principalTable: "Contents",
                        principalColumn: "ContentId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContentCreator_Creators_CreatorsCreatorId",
                        column: x => x.CreatorsCreatorId,
                        principalTable: "Creators",
                        principalColumn: "CreatorId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ContentCreator_CreatorsCreatorId",
                table: "ContentCreator",
                column: "CreatorsCreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentCreator");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Contents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Creators_CreatorId",
                table: "Contents",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "CreatorId");
        }
    }
}
