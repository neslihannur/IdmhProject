using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdmhProject.Migrations
{
    /// <inheritdoc />
    public partial class CategoryId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId1",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CategoryId1",
                table: "Projects",
                column: "CategoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Categories_CategoryId1",
                table: "Projects",
                column: "CategoryId1",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Categories_CategoryId1",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CategoryId1",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "Projects");
        }
    }
}
