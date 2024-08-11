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

    }
}
