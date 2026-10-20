using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace CodeforcesTracker.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Problem> Problems { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ProblemTag> ProblemTags { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình Composite Key (Khóa chính kép) cho bảng trung gian ProblemTags
            modelBuilder.Entity<ProblemTag>()
                .HasKey(pt => new { pt.ProblemId, pt.TagId });
        }
    }
}