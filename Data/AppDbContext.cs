using IdmhProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ParentCategory> ParentCategories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactFormSubmission> ContactFormSubmissions { get; set; }
        public DbSet<Project> Projects { get; set; }

        public DbSet<StaticContent> StaticContents { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }

        public DbSet<Career> Career { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Projects)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory) // Category'nin ParentCategory ile ilişkisi
                .WithMany(pc => pc.SubCategories) // ParentCategory'nin birden fazla alt kategoriye sahip olduğunu belirtir
                .HasForeignKey(c => c.ParentCategoryId); // Yabancı anahtar
        }
    }
}
