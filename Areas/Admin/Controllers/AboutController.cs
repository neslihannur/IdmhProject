using IdmhProject.Data;
using IdmhProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IdmhProject.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AboutController : Controller
    {
        private readonly AppDbContext _context;

        public AboutController(AppDbContext context)
        {
            _context = context;
        }

        
        private bool SessionCheck()
        {
            return HttpContext.Session.GetString("IsAuthenticated") == "true";
        }

        public async Task<IActionResult> Index()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            ViewBag.Username = HttpContext.Session.GetString("Username");
            var teamMembers = await _context.TeamMembers.ToListAsync();
            return View(teamMembers);
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

            var teamMember = await _context.TeamMembers.FirstOrDefaultAsync(m => m.Id == id);
            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }
       
        public IActionResult Create()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Position,Bio,Image,ImageFile")] TeamMember teamMember)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                if (teamMember.ImageFile != null)
                {
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip");

                    if (!Directory.Exists(wwwrootPath))
                    {
                        Directory.CreateDirectory(wwwrootPath);
                    }

                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(teamMember.ImageFile.FileName);
                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await teamMember.ImageFile.CopyToAsync(stream);
                    }

                    teamMember.Image = newFileName;
                }

                _context.Add(teamMember);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(teamMember);
        }

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

            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Position,Bio,ImageFile")] TeamMember teamMember)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id != teamMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingMember = await _context.TeamMembers.FindAsync(id);
                    if (existingMember == null)
                    {
                        return NotFound();
                    }

                    if (teamMember.ImageFile != null)
                    {
                        string existingImage = existingMember.Image;

                        if (!string.IsNullOrEmpty(existingImage))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip", existingImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip");

                        if (!Directory.Exists(wwwrootPath))
                        {
                            Directory.CreateDirectory(wwwrootPath);
                        }

                        string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(teamMember.ImageFile.FileName);
                        string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            await teamMember.ImageFile.CopyToAsync(stream);
                        }

                        existingMember.Image = newFileName;
                    }

                    existingMember.Name = teamMember.Name;
                    existingMember.Position = teamMember.Position;
                    existingMember.Bio = teamMember.Bio;

                    _context.Entry(existingMember).State = EntityState.Modified;
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeamMemberExists(teamMember.Id))
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

            return View(teamMember);
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

            var teamMember = await _context.TeamMembers.FirstOrDefaultAsync(m => m.Id == id);
            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember != null)
            {
                _context.TeamMembers.Remove(teamMember);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TeamMemberExists(int id)
        {
            return _context.TeamMembers.Any(e => e.Id == id);
        }

        public IActionResult Career()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var careers = _context.Career.ToList();
            if (careers == null || !careers.Any())
            {
                return View("Career");
            }

            return View(careers);
        }

        public IActionResult CareerCreate()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CareerCreate([Bind("Id,Profile,Award")] Career career)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                _context.Add(career);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Career));
            }

            return View(career);
        }

        [HttpPost]
        public IActionResult DeleteCareer(int id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var career = _context.Career.FirstOrDefault(c => c.Id == id);
            if (career == null)
            {
                return NotFound();
            }

            _context.Career.Remove(career);
            _context.SaveChanges();

            return RedirectToAction("Career");
        }
    }
}
