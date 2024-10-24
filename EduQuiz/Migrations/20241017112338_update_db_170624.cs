using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_170624 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "GroupPost",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "GroupPost");
        }
    }
}
