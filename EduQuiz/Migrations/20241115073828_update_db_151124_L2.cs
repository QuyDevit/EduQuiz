using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124_L2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EduQuizId",
                table: "EduQuizSnapshot",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EduQuizSnapshot_EduQuizId",
                table: "EduQuizSnapshot",
                column: "EduQuizId");

            migrationBuilder.AddForeignKey(
                name: "FK_EduQuizSnapshot_EduQuiz_EduQuizId",
                table: "EduQuizSnapshot",
                column: "EduQuizId",
                principalTable: "EduQuiz",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EduQuizSnapshot_EduQuiz_EduQuizId",
                table: "EduQuizSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_EduQuizSnapshot_EduQuizId",
                table: "EduQuizSnapshot");

            migrationBuilder.DropColumn(
                name: "EduQuizId",
                table: "EduQuizSnapshot");
        }
    }
}
