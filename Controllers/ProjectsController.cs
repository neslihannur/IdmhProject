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

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Projects.Include(p => p.Category);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Projects/Details/5
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

        // GET: Projects/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Image,CreatedDate,CategoryId,TeamMember,ImageFile")] Project project)
        {
            string? newFileName = null;

            if (project.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(project.ImageFile.FileName);
                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");

                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    project.ImageFile.CopyTo(stream);
                }

                Project item = new Project()
                {
                    Title = project.Title,
                    Description = project.Description,
                    Image = newFileName,
                    TeamMember = project.TeamMember,
                    CreatedDate = DateTime.Now,
                    
                    CategoryId = project.CategoryId,
                    
                };

                // Projeyi ekliyoruz
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Eğer model geçerli değilse
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", project.CategoryId);
            return View(project);
        }


        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", project.CategoryId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,  Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Model geçerli değilse, kategori listesini yeniden oluştur
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", project.CategoryId);
                return View(project);
            }

            try
            {
                // Mevcut projeyi veritabanından al
                var existingProject = await _context.Projects.FindAsync(id);
                if (existingProject == null)
                {
                    return NotFound();
                }

                string newFileName = existingProject.Image; // Eski dosya adını koru

                // Yeni bir resim yüklendiyse
                if (project.ImageFile != null)
                {
                    // Eğer eski bir fotoğraf varsa, önce onu sil
                    if (!string.IsNullOrEmpty(existingProject.Image))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages", existingProject.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);  // Eski fotoğrafı sil
                        }
                    }

                    // Yeni dosya adını oluştur
                    newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(project.ImageFile.FileName);
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");

                    // Klasör yoksa oluştur
                    if (!Directory.Exists(wwwrootPath))
                    {
                        Directory.CreateDirectory(wwwrootPath);
                    }

                    // Yeni fotoğrafın tam yolunu oluştur
                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await project.ImageFile.CopyToAsync(stream);
                    }
                }

                // Veritabanındaki item bilgilerini güncelle
                existingProject.Title = project.Title;
                existingProject.Description = project.Description;
                existingProject.Image = newFileName; // Yeni dosya adını güncelle
                existingProject.TeamMember = project.TeamMember;
                existingProject.CreatedDate = project.CreatedDate;
                existingProject.CategoryId = project.CategoryId;

                // Projeyi güncelle
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


        // GET: Projects/Delete/5
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

        // POST: Projects/Delete/5
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
