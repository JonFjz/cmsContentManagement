using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cmsContentManagment.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatorToCreators : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Creator_CreatorId",
                table: "Contents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Creator",
                table: "Creator");

            migrationBuilder.RenameTable(
                name: "Creator",
                newName: "Creators");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Creators",
                table: "Creators",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Creators_CreatorId",
                table: "Contents",
                column: "CreatorId",
                principalTable: "Creators",
                principalColumn: "CreatorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contents_Creators_CreatorId",
                table: "Contents");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Creators",
                table: "Creators");

            migrationBuilder.RenameTable(
                name: "Creators",
                newName: "Creator");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Creator",
                table: "Creator",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contents_Creator_CreatorId",
                table: "Contents",
                column: "CreatorId",
                principalTable: "Creator",
                principalColumn: "CreatorId");
        }
    }
}
