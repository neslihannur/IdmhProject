
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

        // GET: Admin/Projects
        public IActionResult Index()
        {
            // Projeleri kategorileri ve takım üyeleri ile birlikte getir
            var projects = _context.Projects
                                   .Include(p => p.Categories)
                                   .Include(p => p.ProjectTeamMembers)
                                       .ThenInclude(ptm => ptm.TeamMember)
                                   .OrderByDescending(p => p.Id)
                                   .ToList();

            // Kategori isimlerini ViewBag'e atıyoruz (isteğe bağlı, View'da kullanılabilir)
            var allCategories = _context.Categories
                                        .Select(c => c.Name)
                                        .Distinct()
                                        .ToList();
            ViewBag.Models = allCategories;

            return View(projects);
        }


        // GET: Admin/Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        public async Task<IActionResult> Create()
        {
            // Kategorileri getir ve SelectList oluştur
            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewData["Categories"] = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();

            // Takım üyelerini getir ve SelectList oluştur
            var teamMembers = await _context.TeamMembers.OrderBy(tm => tm.Name).ToListAsync();
            ViewData["TeamMembers"] = teamMembers.Select(tm => new SelectListItem
            {
                Value = tm.Id.ToString(),
                Text = tm.Name
            }).ToList();

            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Create(Project project, List<int> selectedTeamMembers)
        {
            if (ModelState.IsValid)
            {
                // Görsel yükleme işlemi
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
                        await project.ImageFile.CopyToAsync(stream);
                    }

                    // Görsel adı veritabanında saklanacak
                    project.Image = newFileName;
                }

                // Projeyi kaydet
                project.CreatedDate = DateTime.Now; // Oluşturma tarihi
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();


                Project projects = new Project()
                {
                    Id = project.Id,
                    Title = project.Title,
                    Image = newFileName,
                    
                    CreatedDate = DateTime.Now,
                    Description = project.Description,
                    CategoryId = project.CategoryId,
                    
                };

                _context.Projects.Add(projects);
                _context.SaveChanges();


                // Seçilen takım üyelerini projeye ekle
                foreach (var memberId in selectedTeamMembers)
                {
                    var projectTeamMember = new ProjectTeamMember
                    {
                        ProjectId = project.Id,
                        TeamMemberId = memberId
                    };
                    _context.ProjectTeamMembers.Add(projectTeamMember);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda kategorileri ve üyeleri tekrar yükle
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", project.CategoryId);
            ViewData["TeamMembers"] = new SelectList(_context.TeamMembers, "Id", "Name");

            return View(project);
        }


        // GET: Admin/Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                                        .Include(p => p.ProjectTeamMembers)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            // Mevcut kategori ve takım üyelerini listele
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", project.CategoryId);
            ViewData["TeamMembers"] = new SelectList(_context.TeamMembers, "Id", "Name");

            return View(project);
        }

        // POST: Admin/Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Image,CreatedDate,CategoryId")] Project project, int[] selectedTeamMembers)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Projeyi güncelle
                    _context.Update(project);
                    await _context.SaveChangesAsync();

                    // Önce mevcut takım üyelerini kaldır
                    var existingMembers = _context.ProjectTeamMembers.Where(ptm => ptm.ProjectId == project.Id);
                    _context.ProjectTeamMembers.RemoveRange(existingMembers);
                    await _context.SaveChangesAsync();

                    // Seçilen takım üyelerini ekle
                    foreach (var memberId in selectedTeamMembers)
                    {
                        var projectTeamMember = new ProjectTeamMember
                        {
                            ProjectId = project.Id,
                            TeamMemberId = memberId
                        };
                        _context.ProjectTeamMembers.Add(projectTeamMember);
                    }

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

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", project.CategoryId);
            ViewData["TeamMembers"] = new SelectList(_context.TeamMembers, "Id", "Name");
            return View(project);
        }


        // GET: Admin/Projects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                                        .Include(p => p.Categories)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Admin/Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                // Proje ile ilgili takım üyelerini sil
                var projectTeamMembers = _context.ProjectTeamMembers.Where(ptm => ptm.ProjectId == project.Id);
                _context.ProjectTeamMembers.RemoveRange(projectTeamMembers);

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
