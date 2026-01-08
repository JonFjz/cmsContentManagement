using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmsContentManagment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatorEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatorId",
                table: "Contents",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Contents",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Creator",
                columns: table => new
                {
                    CreatorId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Creator", x => x.CreatorId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Creator_CreatorId",
                table: "Contents",
                column: "CreatorId",
                principalTable: "Creator",
                principalColumn: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Creator_CreatorId",
                table: "Contents");

            migrationBuilder.DropTable(
                name: "Creator");

            migrationBuilder.DropIndex(
                name: "IX_Contents_CreatorId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Contents");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Contents");
        }
    }
}
