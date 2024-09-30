using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_240924 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAuto",
                table: "QuizSession",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRandomAnswer",
                table: "QuizSession",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRandomQuestion",
                table: "QuizSession",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowAvatar",
                table: "QuizSession",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShowQuestionAndAnswer",
                table: "QuizSession",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAuto",
                table: "QuizSession");

            migrationBuilder.DropColumn(
                name: "IsRandomAnswer",
                table: "QuizSession");

            migrationBuilder.DropColumn(
                name: "IsRandomQuestion",
                table: "QuizSession");

            migrationBuilder.DropColumn(
                name: "IsShowAvatar",
                table: "QuizSession");

            migrationBuilder.DropColumn(
                name: "IsShowQuestionAndAnswer",
                table: "QuizSession");
        }
    }
}
