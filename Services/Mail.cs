using System.Net;
using System.Net.Mail;
using System.Text;
using IdmhProject.Data;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Services;

/// E-postalar önce giden kutusuna yazılır; OutboxWorker gönderir ve hata olursa tekrar dener.
public class Mailer(AppDb db, IConfiguration config)
{
    string BaseUrl => (config["Site:BaseUrl"] ?? "").TrimEnd('/');

    void Add(string to, string subject, string html, string? replyTo = null)
    {
        var now = DateTime.UtcNow;
        db.EmailMessages.Add(new EmailMessage { To = to, ReplyTo = replyTo, Subject = subject, Html = html, Status = EmailStatus.Pending, CreatedAt = now, NextAttemptAt = now });
    }

    /// Kaydedilmiş (Id almış) talep için: müşteriye otomatik yanıt, stüdyoya bildirim.
    public void InquiryReceived(Inquiry q)
    {
        var quote = q.Kind == "teklif";
        Add(q.Email, quote ? "Teklif talebinizi aldık — IDMH" : "Mesajınızı aldık — IDMH", MailTemplates.AutoReply(q, BaseUrl));
        if (config["Site:NotifyEmail"] is { Length: > 0 } studio)
            Add(studio, $"{(quote ? "Yeni teklif talebi" : "Yeni mesaj")} · {q.Name}{(q.Service is null ? "" : " · " + q.Service)}",
                MailTemplates.StudioNotice(q, BaseUrl), replyTo: q.Email);
    }
}

public static class MailTemplates
{
    static string H(string? s) => WebUtility.HtmlEncode(s ?? "");

    static string Layout(string body, string baseUrl) => $"""
        <!doctype html><html lang="tr"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
        <body style="margin:0;background:#EDECE8;font-family:'Helvetica Neue',Helvetica,Arial,sans-serif;color:#111;">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="padding:40px 12px;"><tr><td align="center">
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:600px;background:#fff;">
          <tr><td style="padding:34px 40px;border-bottom:1px solid #111;">
            <a href="{baseUrl}" style="color:#111;text-decoration:none;font-size:22px;letter-spacing:6px;font-weight:700;">IDMH</a>
            <div style="font-size:11px;letter-spacing:2px;color:#777;margin-top:4px;text-transform:uppercase;">İbrahim Durmaz Mimarlık Hizmetleri</div>
          </td></tr>
          <tr><td style="padding:40px;font-size:15px;line-height:1.7;">{body}</td></tr>
          <tr><td style="padding:24px 40px 34px;border-top:1px solid #E2E0DA;font-size:12px;line-height:1.7;color:#777;">
            Abdurrahmangazi Mah. Fatih Bulvarı No:71 D:1, Sultanbeyli / İstanbul · +90 540 436 40 24<br>
            Bu e-posta bir portföy tasarım çalışmasından gönderildi.
          </td></tr>
        </table></td></tr></table></body></html>
        """;

    static string Row(string k, string? v) => string.IsNullOrWhiteSpace(v) ? "" :
        $"""<tr><td style="padding:9px 0;border-bottom:1px solid #EEE;color:#777;font-size:13px;width:38%;">{H(k)}</td><td style="padding:9px 0;border-bottom:1px solid #EEE;font-size:14px;">{H(v)}</td></tr>""";

    static string Details(Inquiry q) => $"""
        <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="margin:22px 0;">
          {Row("Hizmet", q.Service)}{Row("Yapı türü", q.Typology)}{Row("Alan", q.Area)}{Row("Konum", q.Location)}{Row("Bütçe", q.Budget)}{Row("Zamanlama", q.Timeline)}
        </table>
        <div style="padding:18px 20px;background:#F5F4F0;font-size:14px;white-space:pre-line;">{H(q.Message)}</div>
        """;

    public static string AutoReply(Inquiry q, string baseUrl)
    {
        var first = q.Name.Trim().Split(' ')[0];
        var body = $"""
            <p style="margin:0 0 6px;font-size:11px;letter-spacing:2px;text-transform:uppercase;color:#777;">{(q.Kind == "teklif" ? "Teklif talebi" : "İletişim")}</p>
            <h1 style="margin:0 0 18px;font-size:26px;font-weight:400;line-height:1.3;">Teşekkürler {H(first)}, talebiniz bize ulaştı.</h1>
            <p style="margin:0;color:#444;">{(q.Kind == "teklif"
                ? "Projenizi inceleyip 2 iş günü içinde sizinle iletişime geçeceğiz. İlk görüşmede ihtiyaç programını, bütçeyi ve takvimi birlikte netleştiriyoruz."
                : "Mesajınızı aldık; en kısa sürede dönüş yapacağız.")}</p>
            {Details(q)}
            <p style="margin:26px 0 0;"><a href="{baseUrl}/projeler" style="display:inline-block;padding:13px 26px;background:#111;color:#fff;text-decoration:none;font-size:13px;letter-spacing:1px;">PROJELERİMİZE GÖZ ATIN</a></p>
            """;
        return Layout(body, baseUrl);
    }

    public static string StudioNotice(Inquiry q, string baseUrl)
    {
        var body = $"""
            <h1 style="margin:0 0 6px;font-size:22px;font-weight:400;">{(q.Kind == "teklif" ? "Yeni teklif talebi" : "Yeni mesaj")}</h1>
            <p style="margin:0;color:#444;">{H(q.Name)} · <a href="mailto:{H(q.Email)}" style="color:#111;">{H(q.Email)}</a>{(string.IsNullOrEmpty(q.Phone) ? "" : " · " + H(q.Phone))}</p>
            {Details(q)}
            <p style="margin:26px 0 0;"><a href="{baseUrl}/yonetim/talepler/{q.Id}" style="display:inline-block;padding:13px 26px;background:#111;color:#fff;text-decoration:none;font-size:13px;letter-spacing:1px;">PANELDE AÇ</a></p>
            """;
        return Layout(body, baseUrl);
    }
}

public class OutboxWorker(IServiceScopeFactory scopes, IConfiguration config, ILogger<OutboxWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stop)
    {
        while (!stop.IsCancellationRequested)
        {
            try { await SendPendingAsync(stop); }
            catch (Exception ex) when (!stop.IsCancellationRequested) { log.LogError(ex, "Giden kutusu işlenemedi"); }
            await Task.Delay(TimeSpan.FromSeconds(10), stop).ContinueWith(_ => { });
        }
    }

    async Task SendPendingAsync(CancellationToken stop)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDb>();
        var now = DateTime.UtcNow;
        var batch = await db.EmailMessages.Where(m => m.Status == EmailStatus.Pending && m.NextAttemptAt <= now).OrderBy(m => m.Id).Take(20).ToListAsync(stop);
        if (batch.Count == 0) return;

        var smtp = config.GetSection("Smtp");
        var pickup = smtp["PickupDirectory"];
        var configured = !string.IsNullOrEmpty(smtp["Host"]) || !string.IsNullOrEmpty(pickup);
        foreach (var m in batch)
        {
            if (!configured) { m.Status = EmailStatus.Skipped; m.LastError = "SMTP ayarlanmamış"; continue; }
            try
            {
                using var msg = new MailMessage
                {
                    From = new MailAddress(smtp["FromEmail"] is { Length: > 0 } f ? f : smtp["User"] is { Length: > 0 } u ? u : "idmh@localhost", smtp["FromName"] ?? "IDMH Mimarlık", Encoding.UTF8),
                    Subject = m.Subject, SubjectEncoding = Encoding.UTF8, Body = m.Html, BodyEncoding = Encoding.UTF8, IsBodyHtml = true,
                };
                msg.To.Add(m.To);
                if (m.ReplyTo is { Length: > 0 } rt) msg.ReplyToList.Add(rt);
                using var client = new SmtpClient();
                if (!string.IsNullOrEmpty(pickup))
                {
                    Directory.CreateDirectory(pickup);
                    client.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    client.PickupDirectoryLocation = Path.GetFullPath(pickup);
                }
                else
                {
                    client.Host = smtp["Host"]!;
                    client.Port = int.TryParse(smtp["Port"], out var port) ? port : 587;
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(smtp["User"], smtp["Password"]);
                }
                await client.SendMailAsync(msg, stop);
                m.Status = EmailStatus.Sent; m.SentAt = DateTime.UtcNow; m.LastError = null;
            }
            catch (Exception ex) when (!stop.IsCancellationRequested)
            {
                m.Attempts++;
                m.LastError = ex.Message;
                m.Status = m.Attempts >= 5 ? EmailStatus.Failed : EmailStatus.Pending;
                m.NextAttemptAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, m.Attempts));
                log.LogWarning(ex, "E-posta gönderilemedi: {To}", m.To);
            }
        }
        await db.SaveChangesAsync(stop);
    }
}
