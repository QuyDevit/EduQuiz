using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_151124 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EduQuizSnapshotId",
                table: "QuizSession",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EduQuizSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageCover = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: true),
                    Visibility = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    ThemeId = table.Column<int>(type: "int", nullable: true),
                    TopicId = table.Column<int>(type: "int", nullable: true),
                    MusicId = table.Column<int>(type: "int", nullable: true),
                    OrderQuestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EduQuizSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EduQuizSnapshot_Interest_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Interest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuizSnapshot_Music_MusicId",
                        column: x => x.MusicId,
                        principalTable: "Music",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuizSnapshot_Theme_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuizSnapshot_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EduQuizId = table.Column<int>(type: "int", nullable: true),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeQuestion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeAnswer = table.Column<int>(type: "int", nullable: true),
                    Time = table.Column<int>(type: "int", nullable: true),
                    PointsMultiplier = table.Column<int>(type: "int", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageEffect = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EduQuizSnapshotId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionSnapshot_EduQuizSnapshot_EduQuizSnapshotId",
                        column: x => x.EduQuizSnapshotId,
                        principalTable: "EduQuizSnapshot",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_QuestionSnapshot_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ChoiceSnapshot",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionId = table.Column<int>(type: "int", nullable: true),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChoiceSnapshot", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChoiceSnapshot_QuestionSnapshot_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "QuestionSnapshot",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizSession_EduQuizSnapshotId",
                table: "QuizSession",
                column: "EduQuizSnapshotId");

            migrationBuilder.CreateIndex(
                name: "IX_ChoiceSnapshot_QuestionId",
                table: "ChoiceSnapshot",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuizSnapshot_MusicId",
                table: "EduQuizSnapshot",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuizSnapshot_ThemeId",
                table: "EduQuizSnapshot",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuizSnapshot_TopicId",
                table: "EduQuizSnapshot",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuizSnapshot_UserId",
                table: "EduQuizSnapshot",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshot_EduQuizId",
                table: "QuestionSnapshot",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionSnapshot_EduQuizSnapshotId",
                table: "QuestionSnapshot",
                column: "EduQuizSnapshotId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuizSession_EduQuizSnapshot_EduQuizSnapshotId",
                table: "QuizSession",
                column: "EduQuizSnapshotId",
                principalTable: "EduQuizSnapshot",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizSession_EduQuizSnapshot_EduQuizSnapshotId",
                table: "QuizSession");

            migrationBuilder.DropTable(
                name: "ChoiceSnapshot");

            migrationBuilder.DropTable(
                name: "QuestionSnapshot");

            migrationBuilder.DropTable(
                name: "EduQuizSnapshot");

            migrationBuilder.DropIndex(
                name: "IX_QuizSession_EduQuizSnapshotId",
                table: "QuizSession");

            migrationBuilder.DropColumn(
                name: "EduQuizSnapshotId",
                table: "QuizSession");
        }
    }
}
