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
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Categories
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Categories.Include(c => c.ParentCategory);
            return View(await appDbContext.ToListAsync());
        }
        public JsonResult GetSubCategories(int parentId)
        {
            var subCategories = _context.Categories
                .Where(c => c.ParentCategoryId == parentId)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToList();

            return Json(subCategories);
        }

        // GET: Admin/Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        public IActionResult Create()
        {
            ViewBag.ParentCategories = new SelectList(_context.ParentCategories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category, string newCategoryName, string newParentCategoryName)
        {
            if (ModelState.IsValid)
            {
                // Yeni bir kategori ekleniyorsa
                if (!string.IsNullOrWhiteSpace(newCategoryName))
                {
                    category.Name = newCategoryName; // Kategori ismini ayarla
                    _context.Categories.Add(category); // Kategoriyi ekle
                }

                // Yeni bir üst kategori ekleniyorsa
                if (!string.IsNullOrWhiteSpace(newParentCategoryName))
                {
                    var parentCategory = new ParentCategory { Name = newParentCategoryName }; // Yeni üst kategori oluştur
                    _context.ParentCategories.Add(parentCategory); // Üst kategoriyi ekle
                    _context.SaveChanges(); // Değişiklikleri kaydet

                    // Eklenen üst kategorinin ID'sini kullan
                    category.ParentCategoryId = parentCategory.Id;
                }

                _context.SaveChanges(); // Kategoriyi kaydet
                return RedirectToAction("Index"); // Başka bir sayfaya yönlendirin
            }

            // Hatalar varsa tekrar formu göster
            return View(category);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            ViewData["ParentCategoryId"] = new SelectList(_context.ParentCategories, "Id", "Id", category.ParentCategoryId);
            return View(category);
        }

        // POST: Admin/Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentCategoryId")] Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            ViewData["ParentCategoryId"] = new SelectList(_context.ParentCategories, "Id", "Id", category.ParentCategoryId);
            return View(category);
        }

        // GET: Admin/Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Admin/Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.Id == id);
        }
    }
}
