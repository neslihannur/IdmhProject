using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Data;

public class Project
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    /// Mimarlık · İç Mimarlık · Mimari Revizyon
    public string Discipline { get; set; } = "Mimarlık";
    /// Konut, ofis, konaklama…
    public string Typology { get; set; } = "";
    public string Location { get; set; } = "";
    public string Year { get; set; } = "";
    public string Area { get; set; } = "";
    public string Status { get; set; } = "Tamamlandı";
    public string Summary { get; set; } = "";
    /// Boş satır = yeni paragraf.
    public string Body { get; set; } = "";
    public string Cover { get; set; } = "";
    public List<string> Gallery { get; set; } = [];
    public bool IsPublished { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class JournalPost
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Excerpt { get; set; } = "";
    public string Body { get; set; } = "";
    public string Cover { get; set; } = "";
    public string Tag { get; set; } = "";
    public DateOnly PublishedOn { get; set; }
    public bool IsPublished { get; set; } = true;
    public int ReadMinutes => Math.Max(2, Body.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length / 180);
}

public class Inquiry
{
    public int Id { get; set; }
    /// "teklif" ya da "iletisim"
    public string Kind { get; set; } = "iletisim";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public string? Service { get; set; }
    public string? Typology { get; set; }
    public string? Area { get; set; }
    public string? Budget { get; set; }
    public string? Location { get; set; }
    public string? Timeline { get; set; }
    public string Message { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? Ip { get; set; }
}

public class MediaFile
{
    public Guid Id { get; set; }
    public string ContentType { get; set; } = "";
    public string FileName { get; set; } = "";
    public byte[] Data { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

public enum EmailStatus { Pending, Sent, Failed, Skipped }

public class EmailMessage
{
    public int Id { get; set; }
    public string To { get; set; } = "";
    public string? ReplyTo { get; set; }
    public string Subject { get; set; } = "";
    public string Html { get; set; } = "";
    public EmailStatus Status { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime NextAttemptAt { get; set; }
    public DateTime? SentAt { get; set; }
}

public class AdminUser
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public DateTime? LastLoginAt { get; set; }
}

public class AppDb(DbContextOptions<AppDb> options) : DbContext(options), IDataProtectionKeyContext
{
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<JournalPost> Posts => Set<JournalPost>();
    public DbSet<Inquiry> Inquiries => Set<Inquiry>();
    public DbSet<MediaFile> Media => Set<MediaFile>();
    public DbSet<EmailMessage> EmailMessages => Set<EmailMessage>();
    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Project>().HasIndex(p => p.Slug).IsUnique();
        b.Entity<JournalPost>().HasIndex(p => p.Slug).IsUnique();
        b.Entity<JournalPost>().Ignore(p => p.ReadMinutes);
        b.Entity<EmailMessage>().Property(m => m.Status).HasConversion<string>().HasMaxLength(16);
        b.Entity<EmailMessage>().HasIndex(m => new { m.Status, m.NextAttemptAt });
        b.Entity<AdminUser>().HasIndex(u => u.Email).IsUnique();
    }
}
