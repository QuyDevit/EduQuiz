using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_021024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizSessionQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizSessionId = table.Column<int>(type: "int", nullable: true),
                    QuestionId = table.Column<int>(type: "int", nullable: true),
                    Order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSessionQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSessionQuestion_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuizSessionQuestion_QuizSession_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestion_QuestionId",
                table: "QuizSessionQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestion_QuizSessionId",
                table: "QuizSessionQuestion",
                column: "QuizSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizSessionQuestion");
        }
    }
}
