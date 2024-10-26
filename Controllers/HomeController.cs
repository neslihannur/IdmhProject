using IdmhProject.Data;
using IdmhProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace IdmhProject.Controllers
{
    public class HomeController : Controller
    {
            private readonly AppDbContext _context;

            public HomeController(AppDbContext context)
            {
                _context = context;
            }


            public IActionResult Index()
            {
                return View();
            }    
             public IActionResult Hizmetlerimiz()
            {
                return View();
            } 
             public IActionResult Kariyer()
            {
                return View();
            }
        public IActionResult BlogDetails(int id)
        {
            var blog = _context.Blogs.FirstOrDefault(b => b.Id == id);

            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        public async Task<IActionResult> Blog()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Author)
                .OrderByDescending(b => b.PublishedDate)
                .ToListAsync();

            return View(blogs);
        }

        public IActionResult Details(int id)
        {
            var project = _context.Projects.Find(id); // Veritabanýndan proje bilgilerini al
            if (project == null)
            {
                return NotFound(); // Proje bulunamazsa 404 döndür
            }
            return View(project); // Proje bilgilerini view'a geçir
        }

        public IActionResult Architecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == 1) // Mimarlýk kategorisi
                    .ToList();

                return View(projects);
            }

            public IActionResult InteriorArchitecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == 2) // Ýç Mimarlýk kategorisi
                    .ToList();

                return View(projects);
            }

        public async Task<IActionResult> TeamMembers()
        {
            var teamMembers = await _context.TeamMembers.ToListAsync();
            return View(teamMembers);
        }
        public IActionResult Hakkimizda()
            {
                return View();
            } public IActionResult Iletisim()
            {
                return View();
            }
        }
    }
