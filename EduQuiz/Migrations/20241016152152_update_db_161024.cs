using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_161024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "AssignmentGroup",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentGroup_UserId",
                table: "AssignmentGroup",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentGroup_User_UserId",
                table: "AssignmentGroup",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentGroup_User_UserId",
                table: "AssignmentGroup");

            migrationBuilder.DropIndex(
                name: "IX_AssignmentGroup_UserId",
                table: "AssignmentGroup");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AssignmentGroup");
        }
    }
}
