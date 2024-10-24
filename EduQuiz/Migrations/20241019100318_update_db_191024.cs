using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_191024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerQuizSessionQuestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerSessionId = table.Column<int>(type: "int", nullable: true),
                    ListQuestionId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerQuizSessionQuestion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerQuizSessionQuestion_PlayerSession_PlayerSessionId",
                        column: x => x.PlayerSessionId,
                        principalTable: "PlayerSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuizSessionQuestion_PlayerSessionId",
                table: "PlayerQuizSessionQuestion",
                column: "PlayerSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerQuizSessionQuestion");
        }
    }
}
