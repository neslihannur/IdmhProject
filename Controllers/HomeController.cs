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
            var project = _context.Projects.Find(id); // Veritaban�ndan proje bilgilerini al
            if (project == null)
            {
                return NotFound(); // Proje bulunamazsa 404 d�nd�r
            }
            return View(project); // Proje bilgilerini view'a ge�ir
        }

        public IActionResult Architecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == 1) // Mimarl�k kategorisi
                    .ToList();

                return View(projects);
            }

            public IActionResult InteriorArchitecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Category)
                    .Where(p => p.CategoryId == 2) // �� Mimarl�k kategorisi
                    .ToList();

                return View(projects);
            }



            public IActionResult Privacy()
            {
                return View();
            }
        }
    }
