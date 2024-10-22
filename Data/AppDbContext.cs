using IdmhProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Author> Authors { get; set; }
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ContactFormSubmission> ContactFormSubmissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        
        public DbSet<StaticContent> StaticContents {  get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Project>()
                .HasOne(p => p.Category) // "Categories" yerine "Category" yazdık
                .WithMany(c => c.Projects) // Category'nin birden fazla projeye sahip olduğunu belirtir
                .HasForeignKey(p => p.CategoryId); // Yabancı anahtar
        }

    }
}
