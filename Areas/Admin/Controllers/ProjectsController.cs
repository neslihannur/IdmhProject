using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IdmhProject.Data;
using IdmhProject.Models;

namespace IdmhProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

        // Merkezi session kontrol yöntemi
        private bool SessionCheck()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        public IActionResult Home()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var user = HttpContext.Session.GetString("Username");
            var userModel = new User { UserName = user };
            return View(userModel);
        }

        public async Task<IActionResult> Index()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var projects = await _context.Projects.Include(p => p.Category).ToListAsync();
            ViewBag.Username = HttpContext.Session.GetString("Username");
            return View(projects);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.Include(p => p.Category).FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpGet]
        public JsonResult GetSubCategories(int parentId)
        {
            var subCategories = _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(subCategories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var parentCategories = _context.ParentCategories.ToList();
            ViewBag.ParentCategories = new SelectList(parentCategories, "Id", "Name");

            var categories = _context.Categories.Include(c => c.ParentCategory).ToList();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Image,CreatedDate,Content,CategoryId,ParentCategoryId,TeamMember,ImageFiles")] Project project)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            List<string> fileNames = new List<string>();

            if (project.ImageFiles != null && project.ImageFiles.Any())
            {
                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");

                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                foreach (var file in project.ImageFiles)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(file.FileName);
                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    fileNames.Add(newFileName);
                }

                project.Image = string.Join(",", fileNames);

                Project item = new Project
                {
                    Title = project.Title,
                    Description = project.Description,
                    Image = project.Image,
                    TeamMember = project.TeamMember,
                    CreatedDate = DateTime.Now,
                    Content = project.Content,
                    CategoryId = project.CategoryId,
                    ParentCategoryId = project.ParentCategoryId,
                };

                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", project.CategoryId);
            return View(project);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Category)
                .Include(p => p.ParentCategory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            var projectDto = new Project
            {
                Id = project.Id,
                Title = project.Title,
                Description = project.Description,
                Image = project.Image,
                CreatedDate = project.CreatedDate,
                CategoryId = project.CategoryId,
                ParentCategoryId = project.ParentCategoryId,
                TeamMember = project.TeamMember,
                Content = project.Content
            };

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", project.CategoryId);
            ViewData["ParentCategoryId"] = new SelectList(_context.ParentCategories, "Id", "Name", project.ParentCategoryId);
            ViewData["ImageFileName"] = project.Image;

            return View(projectDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id != project.Id)
            {
                return NotFound();
            }

            try
            {
                var existingProject = await _context.Projects.FindAsync(id);
                if (existingProject == null)
                {
                    return NotFound();
                }

                if (project.ImageFiles != null && project.ImageFiles.Any())
                {
                    if (!string.IsNullOrEmpty(existingProject.Image))
                    {
                        var oldImageFiles = existingProject.Image.Split(',');
                        foreach (var oldImage in oldImageFiles)
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages", oldImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }

                    List<string> newFileNames = new List<string>();

                    foreach (var file in project.ImageFiles)
                    {
                        string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(file.FileName);
                        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");

                        if (!Directory.Exists(wwwrootPath))
                        {
                            Directory.CreateDirectory(wwwrootPath);
                        }

                        string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        newFileNames.Add(newFileName);
                    }

                    existingProject.Image = string.Join(",", newFileNames);
                }

                existingProject.Title = project.Title;
                existingProject.Description = project.Description;
                existingProject.TeamMember = project.TeamMember;
                existingProject.CreatedDate = project.CreatedDate;
                existingProject.CategoryId = project.CategoryId;
                existingProject.ParentCategoryId = project.ParentCategoryId;

                _context.Update(existingProject);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
