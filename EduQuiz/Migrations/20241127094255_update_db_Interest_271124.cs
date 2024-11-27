using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_Interest_271124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Interest");

            migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Interest",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Image",
                table: "Interest");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Interest",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
