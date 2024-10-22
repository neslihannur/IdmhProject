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
            public IActionResult Architecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Categories)
                    .Where(p => p.CategoryId == 1) // Mimarlýk kategorisi
                    .ToList();

                return View(projects);
            }

            public IActionResult InteriorArchitecture()
            {
                var projects = _context.Projects
                    .Include(p => p.Categories)
                    .Where(p => p.CategoryId == 2) // Ýç Mimarlýk kategorisi
                    .ToList();

                return View(projects);
            }



            public IActionResult Privacy()
            {
                return View();
            }
        }
    }
