using System.Globalization;
using System.Text;
using IdmhProject.Data;
using Microsoft.AspNetCore.Html;

namespace IdmhProject.Services;

public static class Text
{
    public static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");

    public static string Slugify(string s)
    {
        var map = new Dictionary<char, string> { ['ı'] = "i", ['İ'] = "i", ['ş'] = "s", ['Ş'] = "s", ['ğ'] = "g", ['Ğ'] = "g", ['ü'] = "u", ['Ü'] = "u", ['ö'] = "o", ['Ö'] = "o", ['ç'] = "c", ['Ç'] = "c" };
        var sb = new StringBuilder();
        foreach (var ch in s.Trim())
        {
            if (map.TryGetValue(ch, out var r)) sb.Append(r);
            else if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
        }
        return sb.ToString().Trim('-');
    }

    /// Boş satırla ayrılmış paragrafları <p> olarak basar; "## " ile başlayan satır ara başlık olur.
    public static IHtmlContent Paragraphs(string body)
    {
        var sb = new StringBuilder();
        foreach (var block in body.Replace("\r", "").Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (block.StartsWith("## "))
            {
                // Başlığın hemen altındaki satırlar (boş satır olmadan) paragraf olarak devam eder.
                var nl = block.IndexOf('\n');
                sb.Append("<h2>").Append(System.Net.WebUtility.HtmlEncode(nl < 0 ? block[3..] : block[3..nl])).Append("</h2>");
                if (nl >= 0) sb.Append("<p>").Append(System.Net.WebUtility.HtmlEncode(block[(nl + 1)..].Trim()).Replace("\n", "<br>")).Append("</p>");
            }
            else if (block.StartsWith("> ")) sb.Append("<blockquote>").Append(System.Net.WebUtility.HtmlEncode(block[2..])).Append("</blockquote>");
            else sb.Append("<p>").Append(System.Net.WebUtility.HtmlEncode(block).Replace("\n", "<br>")).Append("</p>");
        }
        return new HtmlString(sb.ToString());
    }

    /// /img/x.jpg için küçük sürüm (/img/x-m.jpg); yüklenen görseller olduğu gibi.
    public static string Small(string url) => url.StartsWith("/img/") && url.EndsWith(".jpg") ? url[..^4] + "-m.jpg" : url;
}

/// Yüklenen görselleri veritabanında saklar (konteyner yeniden kurulunca kaybolmasın).
public class MediaStore(AppDb db)
{
    const long MaxBytes = 12 * 1024 * 1024;

    public async Task<string?> SaveAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > MaxBytes) return null;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var data = ms.ToArray();
        var type = Sniff(data);
        if (type is null) return null;
        var m = new MediaFile { Id = Guid.NewGuid(), ContentType = type, FileName = Path.GetFileName(file.FileName), Data = data, CreatedAt = DateTime.UtcNow };
        db.Media.Add(m);
        await db.SaveChangesAsync();
        return $"/media/{m.Id}";
    }

    static string? Sniff(byte[] d) =>
        d.Length > 12 && d[0] == 0xFF && d[1] == 0xD8 ? "image/jpeg" :
        d.Length > 12 && d[0] == 0x89 && d[1] == 0x50 && d[2] == 0x4E && d[3] == 0x47 ? "image/png" :
        d.Length > 12 && d[0] == 'R' && d[1] == 'I' && d[2] == 'F' && d[3] == 'F' && d[8] == 'W' && d[9] == 'E' ? "image/webp" : null;
}
