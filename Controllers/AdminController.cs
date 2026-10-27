using System.Security.Claims;
using IdmhProject.Data;
using IdmhProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Controllers;

[Route("yonetim"), Authorize]
public class AdminController(AppDb db, MediaStore media) : Controller
{
    static readonly PasswordHasher<AdminUser> Hasher = new();

    // ---------- Giriş ----------
    [HttpGet("giris"), AllowAnonymous]
    public IActionResult Login(string? returnUrl) => View("Login", returnUrl);

    [HttpPost("giris"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string? email, string? password, string? returnUrl)
    {
        var user = await db.AdminUsers.FirstOrDefaultAsync(u => u.Email == (email ?? "").Trim().ToLower());
        var result = user is null ? PasswordVerificationResult.Failed : Hasher.VerifyHashedPassword(user, user.PasswordHash, password ?? "");
        if (user is null || result == PasswordVerificationResult.Failed)
        {
            ViewData["Error"] = "E-posta ya da şifre hatalı.";
            return View("Login", returnUrl);
        }
        user.LastLoginAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        var id = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email)], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(id), new AuthenticationProperties { IsPersistent = true });
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/yonetim");
    }

    [HttpPost("cikis")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/yonetim/giris");
    }

    // ---------- Pano ----------
    [HttpGet("")]
    public async Task<IActionResult> Dashboard()
    {
        ViewData["ProjectCount"] = await db.Projects.CountAsync();
        ViewData["PostCount"] = await db.Posts.CountAsync();
        ViewData["Unread"] = await db.Inquiries.CountAsync(i => !i.IsRead);
        ViewData["Month"] = await db.Inquiries.CountAsync(i => i.CreatedAt >= DateTime.UtcNow.AddDays(-30));
        ViewData["Failed"] = await db.EmailMessages.CountAsync(m => m.Status == EmailStatus.Failed);
        return View("Dashboard", await db.Inquiries.OrderByDescending(i => i.CreatedAt).Take(8).ToListAsync());
    }

    // ---------- Projeler ----------
    [HttpGet("projeler")]
    public async Task<IActionResult> Projects() => View("Projects", await db.Projects.OrderBy(p => p.SortOrder).ToListAsync());

    [HttpGet("projeler/yeni")]
    public async Task<IActionResult> NewProject() => View("Project", new Project { SortOrder = await db.Projects.CountAsync() });

    [HttpGet("projeler/{id:int}")]
    public async Task<IActionResult> EditProject(int id) => await db.Projects.FindAsync(id) is { } p ? View("Project", p) : NotFound();

    [HttpPost("projeler/yeni"), HttpPost("projeler/{id:int}")]
    public async Task<IActionResult> SaveProject(int? id, Project input, string? galleryText, IFormFile? coverFile, List<IFormFile>? galleryFiles)
    {
        var p = id is null ? new Project { CreatedAt = DateTime.UtcNow } : await db.Projects.FindAsync(id.Value);
        if (p is null) return NotFound();
        var slug = Text.Slugify(string.IsNullOrWhiteSpace(input.Slug) ? input.Title ?? "" : input.Slug);
        if (string.IsNullOrWhiteSpace(input.Title)) ViewData["Error"] = "Proje adı gerekli.";
        else if (await db.Projects.AnyAsync(x => x.Slug == slug && x.Id != p.Id)) ViewData["Error"] = $"\"{slug}\" adresi başka bir projede kullanılıyor.";
        if (ViewData["Error"] is not null) { input.Id = p.Id; return View("Project", input); }

        p.Slug = slug; p.Title = input.Title!.Trim(); p.Discipline = input.Discipline; p.Typology = input.Typology?.Trim() ?? "";
        p.Location = input.Location?.Trim() ?? ""; p.Year = input.Year?.Trim() ?? ""; p.Area = input.Area?.Trim() ?? "";
        p.Status = input.Status?.Trim() ?? ""; p.Summary = input.Summary?.Trim() ?? ""; p.Body = input.Body?.Trim() ?? "";
        p.IsPublished = input.IsPublished; p.IsFeatured = input.IsFeatured; p.SortOrder = input.SortOrder;
        p.Gallery = (galleryText ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        p.Cover = input.Cover?.Trim() ?? p.Cover;

        var failed = 0;
        if (coverFile is { Length: > 0 }) { if (await media.SaveAsync(coverFile) is { } url) p.Cover = url; else failed++; }
        foreach (var f in galleryFiles ?? [])
            if (await media.SaveAsync(f) is { } url) p.Gallery.Add(url); else failed++;
        if (string.IsNullOrEmpty(p.Cover) && p.Gallery.Count > 0) p.Cover = p.Gallery[0];

        if (id is null) db.Projects.Add(p);
        await db.SaveChangesAsync();
        TempData["Flash"] = failed > 0 ? $"Kaydedildi; {failed} dosya görsel olarak okunamadı (JPG, PNG ya da WebP, en fazla 12 MB)." : "Proje kaydedildi.";
        return Redirect($"/yonetim/projeler/{p.Id}");
    }

    [HttpPost("projeler/{id:int}/sil")]
    public async Task<IActionResult> DeleteProject(int id)
    {
        if (await db.Projects.FindAsync(id) is { } p) { db.Projects.Remove(p); await db.SaveChangesAsync(); TempData["Flash"] = $"{p.Title} silindi."; }
        return Redirect("/yonetim/projeler");
    }

    // ---------- Yazılar ----------
    [HttpGet("yazilar")]
    public async Task<IActionResult> Posts() => View("Posts", await db.Posts.OrderByDescending(p => p.PublishedOn).ToListAsync());

    [HttpGet("yazilar/yeni")]
    public IActionResult NewPost() => View("Post", new JournalPost { PublishedOn = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(3)) });

    [HttpGet("yazilar/{id:int}")]
    public async Task<IActionResult> EditPost(int id) => await db.Posts.FindAsync(id) is { } p ? View("Post", p) : NotFound();

    [HttpPost("yazilar/yeni"), HttpPost("yazilar/{id:int}")]
    public async Task<IActionResult> SavePost(int? id, JournalPost input, IFormFile? coverFile)
    {
        var p = id is null ? new JournalPost() : await db.Posts.FindAsync(id.Value);
        if (p is null) return NotFound();
        var slug = Text.Slugify(string.IsNullOrWhiteSpace(input.Slug) ? input.Title ?? "" : input.Slug);
        if (string.IsNullOrWhiteSpace(input.Title)) ViewData["Error"] = "Başlık gerekli.";
        else if (await db.Posts.AnyAsync(x => x.Slug == slug && x.Id != p.Id)) ViewData["Error"] = $"\"{slug}\" adresi kullanılıyor.";
        if (ViewData["Error"] is not null) { input.Id = p.Id; return View("Post", input); }

        p.Slug = slug; p.Title = input.Title!.Trim(); p.Tag = input.Tag?.Trim() ?? ""; p.Excerpt = input.Excerpt?.Trim() ?? "";
        p.Body = input.Body?.Trim() ?? ""; p.PublishedOn = input.PublishedOn; p.IsPublished = input.IsPublished;
        p.Cover = input.Cover?.Trim() ?? p.Cover;
        if (coverFile is { Length: > 0 } && await media.SaveAsync(coverFile) is { } url) p.Cover = url;
        if (id is null) db.Posts.Add(p);
        await db.SaveChangesAsync();
        TempData["Flash"] = "Yazı kaydedildi.";
        return Redirect($"/yonetim/yazilar/{p.Id}");
    }

    [HttpPost("yazilar/{id:int}/sil")]
    public async Task<IActionResult> DeletePost(int id)
    {
        if (await db.Posts.FindAsync(id) is { } p) { db.Posts.Remove(p); await db.SaveChangesAsync(); TempData["Flash"] = "Yazı silindi."; }
        return Redirect("/yonetim/yazilar");
    }

    // ---------- Talepler ----------
    [HttpGet("talepler")]
    public async Task<IActionResult> Inquiries() => View("Inquiries", await db.Inquiries.OrderByDescending(i => i.CreatedAt).Take(300).ToListAsync());

    [HttpGet("talepler/{id:int}")]
    public async Task<IActionResult> Inquiry(int id)
    {
        var q = await db.Inquiries.FindAsync(id);
        if (q is null) return NotFound();
        if (!q.IsRead) { q.IsRead = true; await db.SaveChangesAsync(); }
        return View("Inquiry", q);
    }

    [HttpPost("talepler/{id:int}/sil")]
    public async Task<IActionResult> DeleteInquiry(int id)
    {
        if (await db.Inquiries.FindAsync(id) is { } q) { db.Inquiries.Remove(q); await db.SaveChangesAsync(); TempData["Flash"] = "Talep silindi."; }
        return Redirect("/yonetim/talepler");
    }

    // ---------- Şifre ----------
    [HttpGet("sifre")]
    public IActionResult Password() => View("Password");

    [HttpPost("sifre")]
    public async Task<IActionResult> Password(string current, string next, string confirm)
    {
        var user = await db.AdminUsers.FindAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        if (user is null) return Redirect("/yonetim/giris");
        if (Hasher.VerifyHashedPassword(user, user.PasswordHash, current ?? "") == PasswordVerificationResult.Failed) ViewData["Error"] = "Mevcut şifre hatalı.";
        else if ((next ?? "").Length < 10) ViewData["Error"] = "Yeni şifre en az 10 karakter olmalı.";
        else if (next != confirm) ViewData["Error"] = "Yeni şifreler aynı değil.";
        else
        {
            user.PasswordHash = Hasher.HashPassword(user, next!);
            await db.SaveChangesAsync();
            TempData["Flash"] = "Şifre değiştirildi.";
            return Redirect("/yonetim/sifre");
        }
        return View("Password");
    }
}
