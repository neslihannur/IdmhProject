using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdmhProject.Migrations
{
    /// <inheritdoc />
    public partial class AddParentCategoryToProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCategoryId",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ParentCategoryId",
                table: "Projects",
                column: "ParentCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_ParentCategories_ParentCategoryId",
                table: "Projects",
                column: "ParentCategoryId",
                principalTable: "ParentCategories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_ParentCategories_ParentCategoryId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_ParentCategoryId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ParentCategoryId",
                table: "Projects");
        }
    }
}
