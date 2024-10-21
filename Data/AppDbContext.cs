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
        public DbSet<ProjectTeamMember> ProjectTeamMembers { get; set; } // Kavşak Tablosu
        public DbSet<StaticContent> StaticContents {  get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Many-to-Many Relationship between Projects and TeamMembers
            modelBuilder.Entity<ProjectTeamMember>()
                .HasKey(pt => new { pt.ProjectId, pt.TeamMemberId });

            modelBuilder.Entity<ProjectTeamMember>()
                .HasOne(pt => pt.Project)
                .WithMany(p => p.ProjectTeamMembers)
                .HasForeignKey(pt => pt.ProjectId);

            modelBuilder.Entity<ProjectTeamMember>()
                .HasOne(pt => pt.TeamMember)
                .WithMany(t => t.ProjectTeamMembers)
                .HasForeignKey(pt => pt.TeamMemberId);

            modelBuilder.Entity<Project>()
               .HasOne(p => p.Categories)
               .WithMany()
               .HasForeignKey(p => p.CategoryId);
        }
    }
}
