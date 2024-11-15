using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124_L5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSnapshot_EduQuizSnapshot_EduQuizSnapshotId",
                table: "QuestionSnapshot");

            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSnapshot_EduQuiz_EduQuizId",
                table: "QuestionSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_QuestionSnapshot_EduQuizSnapshotId",
                table: "QuestionSnapshot");

            migrationBuilder.DropColumn(
                name: "EduQuizSnapshotId",
                table: "QuestionSnapshot");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSnapshot_EduQuizSnapshot_EduQuizId",
                table: "QuestionSnapshot",
                column: "EduQuizId",
                principalTable: "EduQuizSnapshot",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuestionSnapshot_EduQuizSnapshot_EduQuizId",
                table: "QuestionSnapshot");

            migrationBuilder.AddColumn<int>(
                name: "EduQuizSnapshotId",
                table: "QuestionSnapshot",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshot_EduQuizSnapshotId",
                table: "QuestionSnapshot",
                column: "EduQuizSnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSnapshot_EduQuizSnapshot_EduQuizSnapshotId",
                table: "QuestionSnapshot",
                column: "EduQuizSnapshotId",
                principalTable: "EduQuizSnapshot",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_QuestionSnapshot_EduQuiz_EduQuizId",
                table: "QuestionSnapshot",
                column: "EduQuizId",
                principalTable: "EduQuiz",
                principalColumn: "Id");
        }
    }
}
