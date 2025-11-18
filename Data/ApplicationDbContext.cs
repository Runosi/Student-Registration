using Microsoft.EntityFrameworkCore;
using WebProject.Models;

namespace WebProject.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Prevent EF from trying to auto-generate IDs for STUDENTIDINTERNAL
            modelBuilder.Entity<Student>()
                .Property(s => s.StudentIdInternal)
                .ValueGeneratedNever();
        }
    }
}
