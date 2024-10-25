using EduQuiz.Models.EF;
using Microsoft.EntityFrameworkCore;

namespace EduQuiz.DatabaseContext
{
    public class EduQuizDBContext : DbContext
    {
        public EduQuizDBContext(DbContextOptions<EduQuizDBContext> options) : base(options)
        {
        }
        public DbSet<Role> Roles { get; set; }
        public DbSet<WorkplaceType> WorkplaceTypes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<Music> Musics { get; set; }
        public DbSet<Models.EF.EduQuiz> EduQuizs { get; set; }
        public DbSet<Theme> Themes { get; set; }
        public DbSet<Choice> Choices { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuizFolder> QuizFolders { get; set; }
        public DbSet<QuizSession> QuizSessions { get; set; }
        public DbSet<PlayerSession> PlayerSessions { get; set; }
        public DbSet<PlayerAnswer> PlayerAnswers { get; set; }
        public DbSet<QuizSessionQuestion> QuizSessionQuestions { get; set; }
        public DbSet<FeedbackQuizSession> FeedbackQuizSessions { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<ShareGroup> ShareGroups { get; set; }
        public DbSet<AssignmentGroup> AssignmentGroups { get; set; }
        public DbSet<GroupPost> GroupPosts { get; set; }
        public DbSet<GroupPostLike> GroupPostLikes { get; set; }
        public DbSet<PlayerQuizSessionQuestion> PlayerQuizSessionQuestions { get; set; }
        public DbSet<EduQuizFavorite> EduQuizFavorite { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ cho bảng Folder
            modelBuilder.Entity<Folder>()
                .HasOne(f => f.ParentFolder)
                .WithMany(f => f.ChildFolders)
                .HasForeignKey(f => f.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh xóa đệ quy
            modelBuilder.Entity<Question>()
            .HasMany(q => q.Choices)
            .WithOne(c => c.Question)
            .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
