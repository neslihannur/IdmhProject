using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdmhProject.Migrations
{
    /// <inheritdoc />
    public partial class AddImage2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image2",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image2",
                table: "Projects");
        }
    }
}
