using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124_L4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionQuestion_Question_QuestionId",
                table: "QuizSessionQuestion");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionQuestion_QuestionSnapshot_QuestionId",
                table: "QuizSessionQuestion",
                column: "QuestionId",
                principalTable: "QuestionSnapshot",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSessionQuestion_QuestionSnapshot_QuestionId",
                table: "QuizSessionQuestion");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSessionQuestion_Question_QuestionId",
                table: "QuizSessionQuestion",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id");
        }
    }
}
