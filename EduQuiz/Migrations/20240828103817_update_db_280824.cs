using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_280824 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EduQuiz_Folders_FolderId",
                table: "EduQuiz");

            migrationBuilder.DropIndex(
                name: "IX_EduQuiz_FolderId",
                table: "EduQuiz");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "EduQuiz");

            migrationBuilder.CreateTable(
                name: "QuizFolders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EduQuizId = table.Column<int>(type: "int", nullable: false),
                    FolderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizFolders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizFolders_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizFolders_Folders_FolderId",
                        column: x => x.FolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizFolders_EduQuizId",
                table: "QuizFolders",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizFolders_FolderId",
                table: "QuizFolders",
                column: "FolderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizFolders");

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "EduQuiz",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EduQuiz_FolderId",
                table: "EduQuiz",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_EduQuiz_Folders_FolderId",
                table: "EduQuiz",
                column: "FolderId",
                principalTable: "Folders",
                principalColumn: "Id");
        }
    }
}
