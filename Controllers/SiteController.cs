using System.ComponentModel.DataAnnotations;
using System.Text;
using IdmhProject.Data;
using IdmhProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Controllers;

public class InquiryForm
{
    [Required(ErrorMessage = "Adınızı yazın."), StringLength(80, MinimumLength = 2, ErrorMessage = "Ad 2-80 karakter olmalı.")]
    public string Name { get; set; } = "";
    [Required(ErrorMessage = "E-posta gerekli."), EmailAddress(ErrorMessage = "Geçerli bir e-posta girin."), StringLength(120)]
    public string Email { get; set; } = "";
    [RegularExpression(@"^[0-9 +()\-]{10,20}$", ErrorMessage = "Geçerli bir telefon girin.")]
    public string? Phone { get; set; }
    [StringLength(40)] public string? Service { get; set; }
    [StringLength(40)] public string? Typology { get; set; }
    [StringLength(40)] public string? Area { get; set; }
    [StringLength(40)] public string? Budget { get; set; }
    [StringLength(80)] public string? Location { get; set; }
    [StringLength(40)] public string? Timeline { get; set; }
    [Required(ErrorMessage = "Kısaca projenizden bahsedin."), StringLength(3000, MinimumLength = 10, ErrorMessage = "Mesaj 10-3000 karakter olmalı.")]
    public string Message { get; set; } = "";
    [Range(typeof(bool), "true", "true", ErrorMessage = "Aydınlatma metnini onaylayın.")]
    public bool Consent { get; set; }
    /// Bot tuzağı
    public string? Website { get; set; }
}

public record ProjectPage(Project Project, Project? Next, Project? Prev);

public class SiteController(AppDb db, Mailer mailer, IConfiguration config) : Controller
{
    [HttpGet("/")]
    public async Task<IActionResult> Home()
    {
        ViewData["Projects"] = await db.Projects.Where(p => p.IsPublished && p.IsFeatured).OrderBy(p => p.SortOrder).Take(6).ToListAsync();
        ViewData["Posts"] = await db.Posts.Where(p => p.IsPublished).OrderByDescending(p => p.PublishedOn).Take(3).ToListAsync();
        return View();
    }

    [HttpGet("/projeler")]
    public async Task<IActionResult> Projects(string? alan)
    {
        var q = db.Projects.Where(p => p.IsPublished);
        if (!string.IsNullOrEmpty(alan)) q = q.Where(p => p.Discipline == alan);
        ViewData["Alan"] = alan;
        ViewData["Counts"] = await db.Projects.Where(p => p.IsPublished).GroupBy(p => p.Discipline).Select(g => new { g.Key, C = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.C);
        return View(await q.OrderBy(p => p.SortOrder).ToListAsync());
    }

    [HttpGet("/projeler/{slug}")]
    public async Task<IActionResult> Project(string slug)
    {
        var all = await db.Projects.Where(p => p.IsPublished).OrderBy(p => p.SortOrder).ToListAsync();
        var i = all.FindIndex(p => p.Slug == slug);
        if (i < 0) return NotFound();
        return View(new ProjectPage(all[i], all[(i + 1) % all.Count], all[(i - 1 + all.Count) % all.Count]));
    }

    [HttpGet("/hizmetler")]
    public IActionResult Services() => View();

    [HttpGet("/ofis")]
    public IActionResult Studio() => View();

    [HttpGet("/yazilar")]
    public async Task<IActionResult> Journal() =>
        View(await db.Posts.Where(p => p.IsPublished).OrderByDescending(p => p.PublishedOn).ToListAsync());

    [HttpGet("/yazilar/{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        var post = await db.Posts.FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);
        if (post is null) return NotFound();
        ViewData["More"] = await db.Posts.Where(p => p.IsPublished && p.Id != post.Id).OrderByDescending(p => p.PublishedOn).Take(2).ToListAsync();
        return View(post);
    }

    [HttpGet("/iletisim")]
    public IActionResult Contact(string? hizmet) => View(new InquiryForm { Service = hizmet });

    [HttpPost("/iletisim"), EnableRateLimiting("form")]
    public async Task<IActionResult> Contact(InquiryForm form)
    {
        if (!string.IsNullOrEmpty(form.Website)) return Redirect("/iletisim?gonderildi=1");
        if (!ModelState.IsValid) return View(form);
        var q = new Inquiry
        {
            Kind = string.IsNullOrEmpty(form.Service) || form.Service == "Genel" ? "iletisim" : "teklif",
            Name = form.Name.Trim(), Email = form.Email.Trim().ToLowerInvariant(), Phone = form.Phone?.Trim(),
            Service = form.Service, Typology = form.Typology, Area = form.Area?.Trim(), Budget = form.Budget,
            Location = form.Location?.Trim(), Timeline = form.Timeline, Message = form.Message.Trim(),
            CreatedAt = DateTime.UtcNow, Ip = HttpContext.Connection.RemoteIpAddress?.ToString(),
        };
        db.Inquiries.Add(q);
        await db.SaveChangesAsync();
        mailer.InquiryReceived(q);
        await db.SaveChangesAsync();
        return Redirect("/iletisim?gonderildi=1#form");
    }

    [HttpGet("/gorsel-kaynaklari")]
    public IActionResult Credits() => View();

    [HttpGet("/kvkk")]
    public IActionResult Privacy() => View();

    [HttpGet("/hata")]
    public IActionResult Error(int? kod)
    {
        Response.StatusCode = kod ?? 500;
        return View(kod ?? 500);
    }

    [HttpGet("/robots.txt")]
    public IActionResult Robots() => Content("User-agent: *\nDisallow: /\n", "text/plain");
}
