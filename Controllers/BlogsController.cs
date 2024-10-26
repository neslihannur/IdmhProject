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
    public class BlogsController : Controller
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
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
            var blogs = await _context.Blogs.Include(b => b.Author).ToListAsync();
            return View(blogs);
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

            var blog = await _context.Blogs.Include(b => b.Author).FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }
        public IActionResult AuthorCreate()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AuthorCreate([Bind("Id,Name")] Author author)
        {
            if (ModelState.IsValid)
            {
                _context.Add(author);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // Redirect to the list of authors after creating
            }
            return View(author);
        }

        public IActionResult Create()
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,PublishedDate,AuthorId,ImageFiles")] Blog blog)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            string? imageNames = null;

            if (blog.ImageFiles != null && blog.ImageFiles.Any())
            {
                List<string> fileNames = new List<string>();
                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogImages");

                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                foreach (var imageFile in blog.ImageFiles)
                {
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(imageFile.FileName);
                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    fileNames.Add(newFileName);
                }

                imageNames = string.Join(",", fileNames);
            }

            Blog item = new Blog()
            {
                Title = blog.Title,
                Content = blog.Content,
                Image = imageNames,
                PublishedDate = DateTime.Now,
                AuthorId = blog.AuthorId
            };

            _context.Add(item);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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

            var project = await _context.Blogs.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", project.AuthorId);

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            if (id != blog.Id)
            {
                return NotFound();
            }

            try
            {
                var existingBlog = await _context.Blogs.FindAsync(id);
                if (existingBlog == null)
                {
                    return NotFound();
                }

                string imageNames = existingBlog.Image;

                if (blog.ImageFiles != null && blog.ImageFiles.Any())
                {
                    if (!string.IsNullOrEmpty(existingBlog.Image))
                    {
                        string[] oldImages = existingBlog.Image.Split(',');
                        foreach (var oldImage in oldImages)
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogImages", oldImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                    }

                    List<string> fileNames = new List<string>();
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogImages");

                    if (!Directory.Exists(wwwrootPath))
                    {
                        Directory.CreateDirectory(wwwrootPath);
                    }

                    foreach (var imageFile in blog.ImageFiles)
                    {
                        string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(imageFile.FileName);
                        string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        fileNames.Add(newFileName);
                    }

                    imageNames = string.Join(",", fileNames);
                }

                existingBlog.Title = blog.Title;
                existingBlog.Content = blog.Content;
                existingBlog.Image = imageNames;
                existingBlog.PublishedDate = DateTime.Now;
                existingBlog.AuthorId = blog.AuthorId;

                _context.Update(existingBlog);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogExists(blog.Id))
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

            var blog = await _context.Blogs.Include(b => b.Author).FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!SessionCheck())
            {
                return RedirectToAction("Index", "Login");
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }

}
