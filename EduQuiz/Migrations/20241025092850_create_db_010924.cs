using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduQuiz.Migrations
{
    /// <inheritdoc />
    public partial class create_db_010924 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Interest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interest", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Music",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Music", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Theme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Theme", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkplaceType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkplaceType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: true),
                    LinkToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VerificationCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Organization = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkplaceTypeId = table.Column<int>(type: "int", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProfilePicture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Favorite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefeshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrivacySettings = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguagePreference = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailVerified = table.Column<bool>(type: "bit", nullable: false),
                    ViolationCount = table.Column<int>(type: "int", nullable: false),
                    SubscriptionType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_WorkplaceType_WorkplaceTypeId",
                        column: x => x.WorkplaceTypeId,
                        principalTable: "WorkplaceType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EduQuiz",
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
                    table.PrimaryKey("PK_EduQuiz", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EduQuiz_Interest_TopicId",
                        column: x => x.TopicId,
                        principalTable: "Interest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuiz_Music_MusicId",
                        column: x => x.MusicId,
                        principalTable: "Music",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuiz_Theme_ThemeId",
                        column: x => x.ThemeId,
                        principalTable: "Theme",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EduQuiz_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Folders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false),
                    ParentFolderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Folders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Folders_Folders_ParentFolderId",
                        column: x => x.ParentFolderId,
                        principalTable: "Folders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Folders_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Group",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Uuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InviteCode = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_Group_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Question",
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Question", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Question_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuizSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EduQuizId = table.Column<int>(type: "int", nullable: false),
                    HostUserId = table.Column<int>(type: "int", nullable: false),
                    Pin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsWaitingRoom = table.Column<bool>(type: "bit", nullable: false),
                    IsRandomQuestion = table.Column<bool>(type: "bit", nullable: false),
                    IsRandomAnswer = table.Column<bool>(type: "bit", nullable: false),
                    IsAuto = table.Column<bool>(type: "bit", nullable: false),
                    IsShowQuestionAndAnswer = table.Column<bool>(type: "bit", nullable: false),
                    IsShowAvatar = table.Column<bool>(type: "bit", nullable: false),
                    TypeQuizSession = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizSession_EduQuiz_EduQuizId",
                        column: x => x.EduQuizId,
                        principalTable: "EduQuiz",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizSession_User_HostUserId",
                        column: x => x.HostUserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                name: "Choice",
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
                    table.PrimaryKey("PK_Choice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Choice_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssignmentGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_AssignmentGroup_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FeedbackQuizSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizSessionId = table.Column<int>(type: "int", nullable: true),
                    Rating = table.Column<int>(type: "int", nullable: true),
                    PositiveLearningOutcome = table.Column<bool>(type: "bit", nullable: true),
                    Liked = table.Column<bool>(type: "bit", nullable: true),
                    PositiveFeeling = table.Column<int>(type: "int", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "PlayerSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizSessionId = table.Column<int>(type: "int", nullable: false),
                    Nickname = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AvatarUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Accessory = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalScore = table.Column<int>(type: "int", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rank = table.Column<int>(type: "int", nullable: false),
                    IsPlayer = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSession_QuizSession_QuizSessionId",
                        column: x => x.QuizSessionId,
                        principalTable: "QuizSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "PlayerAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerSessionId = table.Column<int>(type: "int", nullable: false),
                    QuestionId = table.Column<int>(type: "int", nullable: false),
                    ChoiceId = table.Column<int>(type: "int", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false),
                    TimeTaken = table.Column<double>(type: "float", nullable: false),
                    Points = table.Column<int>(type: "int", nullable: false),
                    AnswerText = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerAnswer_Choice_ChoiceId",
                        column: x => x.ChoiceId,
                        principalTable: "Choice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlayerAnswer_PlayerSession_PlayerSessionId",
                        column: x => x.PlayerSessionId,
                        principalTable: "PlayerSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerAnswer_Question_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Question",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_AssignmentGroup_UserId",
                table: "AssignmentGroup",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Choice_QuestionId",
                table: "Choice",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuiz_MusicId",
                table: "EduQuiz",
                column: "MusicId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuiz_ThemeId",
                table: "EduQuiz",
                column: "ThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuiz_TopicId",
                table: "EduQuiz",
                column: "TopicId");

            migrationBuilder.CreateIndex(
                name: "IX_EduQuiz_UserId",
                table: "EduQuiz",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackQuizSession_QuizSessionId",
                table: "FeedbackQuizSession",
                column: "QuizSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_ParentFolderId",
                table: "Folders",
                column: "ParentFolderId");

            migrationBuilder.CreateIndex(
                name: "IX_Folders_UserId",
                table: "Folders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Group_UserId",
                table: "Group",
                column: "UserId");

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
                name: "IX_PlayerAnswer_ChoiceId",
                table: "PlayerAnswer",
                column: "ChoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswer_PlayerSessionId",
                table: "PlayerAnswer",
                column: "PlayerSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerAnswer_QuestionId",
                table: "PlayerAnswer",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerQuizSessionQuestion_PlayerSessionId",
                table: "PlayerQuizSessionQuestion",
                column: "PlayerSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSession_QuizSessionId",
                table: "PlayerSession",
                column: "QuizSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Question_EduQuizId",
                table: "Question",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizFolders_EduQuizId",
                table: "QuizFolders",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizFolders_FolderId",
                table: "QuizFolders",
                column: "FolderId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSession_EduQuizId",
                table: "QuizSession",
                column: "EduQuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSession_HostUserId",
                table: "QuizSession",
                column: "HostUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestion_QuestionId",
                table: "QuizSessionQuestion",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizSessionQuestion_QuizSessionId",
                table: "QuizSessionQuestion",
                column: "QuizSessionId");

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

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_WorkplaceTypeId",
                table: "User",
                column: "WorkplaceTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentGroup");

            migrationBuilder.DropTable(
                name: "FeedbackQuizSession");

            migrationBuilder.DropTable(
                name: "GroupMember");

            migrationBuilder.DropTable(
                name: "GroupPostLike");

            migrationBuilder.DropTable(
                name: "PlayerAnswer");

            migrationBuilder.DropTable(
                name: "PlayerQuizSessionQuestion");

            migrationBuilder.DropTable(
                name: "QuizFolders");

            migrationBuilder.DropTable(
                name: "QuizSessionQuestion");

            migrationBuilder.DropTable(
                name: "ShareGroup");

            migrationBuilder.DropTable(
                name: "GroupPost");

            migrationBuilder.DropTable(
                name: "Choice");

            migrationBuilder.DropTable(
                name: "PlayerSession");

            migrationBuilder.DropTable(
                name: "Folders");

            migrationBuilder.DropTable(
                name: "Group");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "QuizSession");

            migrationBuilder.DropTable(
                name: "EduQuiz");

            migrationBuilder.DropTable(
                name: "Interest");

            migrationBuilder.DropTable(
                name: "Music");

            migrationBuilder.DropTable(
                name: "Theme");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "WorkplaceType");
        }
    }
}
