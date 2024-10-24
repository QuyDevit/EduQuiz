using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_081024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FeedbackQuizSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizSessionId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    PositiveLearningOutcome = table.Column<bool>(type: "bit", nullable: false),
                    Liked = table.Column<bool>(type: "bit", nullable: false),
                    PositiveFeeling = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackQuizSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeedbackQuizSession_QuizSession_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackQuizSession_QuizSessionId",
                table: "FeedbackQuizSession",
                column: "QuizSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FeedbackQuizSession");
        }
    }
}
