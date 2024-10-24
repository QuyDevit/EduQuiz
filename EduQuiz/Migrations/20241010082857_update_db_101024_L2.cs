using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_101024_L2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Group",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId",
                table: "Group",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_User_UserId",
                table: "Group",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Group_User_UserId",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Group_UserId",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Group");
        }
    }
}
