using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IdmhProject.Data;
using IdmhProject.Models;

namespace IdmhProject.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Projects.Include(p => p.Category);
            return View(await appDbContext.ToListAsync());
        }

      
        public async Task<IActionResult> Details(int? id)
        {
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

        
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Image,CreatedDate,CategoryId,TeamMember,ImageFiles")] Project project)
        {
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

                
                Project item = new Project()
                {
                    Title = project.Title,
                    Description = project.Description,
                    Image = project.Image,  
                    TeamMember = project.TeamMember,
                    CreatedDate = DateTime.Now,
                    CategoryId = project.CategoryId,
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
            if (id == null)
            {
                return NotFound();
            }

            // Category ile birlikte proje verisini al
            var project = await _context.Projects.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            // Kategorileri yükle
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", project.CategoryId);

            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
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

                
                string existingImageFiles = existingProject.Image;

                
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
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}
