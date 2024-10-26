using IdmhProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace IdmhProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoginController : Controller
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string username, string password)
        {
            // Kullanıcıyı kontrol etmek için LINQ sorgusu
            var user = _context.Users
                        .FirstOrDefault(u => u.UserName == username && u.Password == password);

            if (user != null)
            {
                // Oturum bilgilerini ayarla
                HttpContext.Session.SetString("IsAuthenticated", "true");
                HttpContext.Session.SetString("Username", user.UserName);

                // Giriş başarılı, Projects sayfasına yönlendir
                return RedirectToAction("Home", "Projects");
            }

            ViewBag.Error = "Geçersiz kullanıcı adı veya şifre!";
            return View();
        }

    }
}
