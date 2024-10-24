using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class update_db_101024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CanInviteNewMembers = table.Column<bool>(type: "bit", nullable: false),
                    CanSeeMemberList = table.Column<bool>(type: "bit", nullable: false),
                    CanShareContent = table.Column<bool>(type: "bit", nullable: false),
                    CanPostContent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Group", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    QuizSessionId = table.Column<int>(type: "int", nullable: true),
                    EduQuizId = table.Column<int>(type: "int", nullable: true),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentGroup_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssignmentGroup_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AssignmentGroup_QuizSession_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSession",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "GroupMember",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    JoinedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupMember_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMember_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPost",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PostedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPost", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPost_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPost_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ShareGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    EduQuizId = table.Column<int>(type: "int", nullable: false),
                    SharedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareGroup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShareGroup_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareGroup_Group_GroupId",
                        column: x => x.GroupId,
                        principalTable: "Group",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShareGroup_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupPostLike",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupPostId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    LikedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupPostLike", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupPostLike_GroupPost_GroupPostId",
                        column: x => x.GroupPostId,
                        principalTable: "GroupPost",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupPostLike_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentGroup_EduQuizId",
                table: "AssignmentGroup",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentGroup_GroupId",
                table: "AssignmentGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentGroup_QuizSessionId",
                table: "AssignmentGroup",
                column: "QuizSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMember_GroupId",
                table: "GroupMember",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupMember_UserId",
                table: "GroupMember",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPost_GroupId",
                table: "GroupPost",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPost_UserId",
                table: "GroupPost",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostLike_GroupPostId",
                table: "GroupPostLike",
                column: "GroupPostId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupPostLike_UserId",
                table: "GroupPostLike",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareGroup_EduQuizId",
                table: "ShareGroup",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareGroup_GroupId",
                table: "ShareGroup",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareGroup_UserId",
                table: "ShareGroup",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentGroup");

            migrationBuilder.DropTable(
                name: "GroupMember");

            migrationBuilder.DropTable(
                name: "GroupPostLike");

            migrationBuilder.DropTable(
                name: "ShareGroup");

            migrationBuilder.DropTable(
                name: "GroupPost");

            migrationBuilder.DropTable(
                name: "Group");
        }
    }
}
