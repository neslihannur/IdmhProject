using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using IdmhProject.Data;
using IdmhProject.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDb>(o => o.UseNpgsql(DatabaseUrl.ConnectionString(builder.Configuration)));
builder.Services.AddDataProtection().PersistKeysToDbContext<AppDb>().SetApplicationName("IdmhProject");
builder.Services.AddScoped<Mailer>();
builder.Services.AddScoped<MediaStore>();
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.Cookie.Name = "idmh_admin";
        o.Cookie.Path = "/yonetim";
        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.LoginPath = "/yonetim/giris";
        o.AccessDeniedPath = "/yonetim/giris";
        o.ExpireTimeSpan = TimeSpan.FromDays(7);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));
builder.Services.AddControllersWithViews(o => o.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 80 * 1024 * 1024);
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 80 * 1024 * 1024);

builder.Services.AddRateLimiter(o =>
{
    o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    static RateLimitPartition<string> Fixed(HttpContext ctx, int limit, TimeSpan window) => RateLimitPartition.GetFixedWindowLimiter(
        ctx.Connection.RemoteIpAddress?.ToString() ?? "?", _ => new FixedWindowRateLimiterOptions { PermitLimit = limit, Window = window });
    o.AddPolicy("login", ctx => Fixed(ctx, 10, TimeSpan.FromMinutes(1)));
    o.AddPolicy("form", ctx => Fixed(ctx, 5, TimeSpan.FromHours(1)));
});

builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDb>();
    await db.Database.MigrateAsync();
    await Seeder.RunAsync(db, app.Configuration, app.Logger);
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/hata");
app.UseStatusCodePagesWithReExecute("/hata", "?kod={0}");

var tr = new CultureInfo("tr-TR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(tr), SupportedCultures = [tr], SupportedUICultures = [tr], RequestCultureProviders = [],
});

app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "SAMEORIGIN";
    h["Referrer-Policy"] = "strict-origin-when-cross-origin";
    // Portföy çalışması: gerçek IDMH sitesinin önüne geçmesin diye arama motorlarına kapalı.
    h["X-Robots-Tag"] = "noindex, nofollow";
    await next();
});

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = c => c.Context.Response.Headers.CacheControl = "public, max-age=604800",
});
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("ok"));
app.MapGet("/media/{id:guid}", async (Guid id, AppDb db, HttpContext ctx) =>
{
    var m = await db.Media.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
    if (m is null) return Results.NotFound();
    ctx.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
    return Results.File(m.Data, m.ContentType);
});
app.MapControllers();

app.Run();
