using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmsContentManagment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserScopeToTagsCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentCreator");

            migrationBuilder.DropTable(
                name: "Creators");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Tags",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Categories",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Categories");

            migrationBuilder.CreateTable(
                name: "Creators",
                columns: table => new
                {
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creators", x => x.CreatorId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
    }
}
