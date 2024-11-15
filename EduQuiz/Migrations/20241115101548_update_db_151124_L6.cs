using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124_L6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentGroup_EduQuiz_EduQuizId",
                table: "AssignmentGroup");

            migrationBuilder.DropColumn(
                name: "ListQuestionId",
                table: "QuizSession");

            migrationBuilder.RenameColumn(
                name: "EduQuizId",
                table: "AssignmentGroup",
                newName: "EduQuizSnapshotId");

            migrationBuilder.RenameIndex(
                name: "IX_AssignmentGroup_EduQuizId",
                table: "AssignmentGroup",
                newName: "IX_AssignmentGroup_EduQuizSnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentGroup_EduQuizSnapshot_EduQuizSnapshotId",
                table: "AssignmentGroup",
                column: "EduQuizSnapshotId",
                principalTable: "EduQuizSnapshot",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssignmentGroup_EduQuizSnapshot_EduQuizSnapshotId",
                table: "AssignmentGroup");

            migrationBuilder.RenameColumn(
                name: "EduQuizSnapshotId",
                table: "AssignmentGroup",
                newName: "EduQuizId");

            migrationBuilder.RenameIndex(
                name: "IX_AssignmentGroup_EduQuizSnapshotId",
                table: "AssignmentGroup",
                newName: "IX_AssignmentGroup_EduQuizId");

            migrationBuilder.AddColumn<string>(
                name: "ListQuestionId",
                table: "QuizSession",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AssignmentGroup_EduQuiz_EduQuizId",
                table: "AssignmentGroup",
                column: "EduQuizId",
                principalTable: "EduQuiz",
                principalColumn: "Id");
        }
    }
}
