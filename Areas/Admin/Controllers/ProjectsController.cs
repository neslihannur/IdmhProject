
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

        public IActionResult Index()
        {
            // Projeleri kategori bilgisiyle birlikte getiriyoruz
            var projects = _context.Projects
                .Include(p => p.Category)  // Category ile ilişkiyi yüklüyoruz
                .ToList();

            return View(projects);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            // Tüm kategorileri almak
            var categories = _context.Categories.ToList();

            // Boş bir Project nesnesi oluşturmak ve kategorileri ViewBag ile view'a taşımak
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(new Project()); // Boş bir Project modelini view'a gönderiyoruz
        }


        // POST: Project/Create
        [HttpPost]
        public async Task<IActionResult> Create(Project project)
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

            _context.Projects.Add(item);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

            
        


        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            ViewData["Categories"] = _context.Categories.ToList();
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    if (project.ImageFile != null)
                    {
                        string newFileName = $"{DateTime.Now:yyyyMMddHHmmssfff}{Path.GetExtension(project.ImageFile.FileName)}";
                        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");
                        string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            await project.ImageFile.CopyToAsync(stream);
                        }

                        project.Image = newFileName;
                    }

                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id)) return NotFound();
                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = _context.Categories.ToList();
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null) return NotFound();

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages", project.Image);
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

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
