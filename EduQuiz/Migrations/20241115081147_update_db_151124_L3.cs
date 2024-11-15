using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124_L3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswer_Choice_ChoiceId",
                table: "PlayerAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswer_Question_QuestionId",
                table: "PlayerAnswer");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswer_ChoiceSnapshot_ChoiceId",
                table: "PlayerAnswer",
                column: "ChoiceId",
                principalTable: "ChoiceSnapshot",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswer_QuestionSnapshot_QuestionId",
                table: "PlayerAnswer",
                column: "QuestionId",
                principalTable: "QuestionSnapshot",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswer_ChoiceSnapshot_ChoiceId",
                table: "PlayerAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerAnswer_QuestionSnapshot_QuestionId",
                table: "PlayerAnswer");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswer_Choice_ChoiceId",
                table: "PlayerAnswer",
                column: "ChoiceId",
                principalTable: "Choice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerAnswer_Question_QuestionId",
                table: "PlayerAnswer",
                column: "QuestionId",
                principalTable: "Question",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
