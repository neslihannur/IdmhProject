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

        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Blogs.Include(b => b.Author);
            return View(await appDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        public IActionResult Create()
        {
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Id");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,PublishedDate,AuthorId,Image,ImageFile")] Blog blog)
        {
            string? newFileName = null;

            if (blog.ImageFile != null)
            {
                newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(blog.ImageFile.FileName);
                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ProjectImages");

                if (!Directory.Exists(wwwrootPath))
                {
                    Directory.CreateDirectory(wwwrootPath);
                }

                string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                using (var stream = new FileStream(imageFullPath, FileMode.Create))
                {
                    blog.ImageFile.CopyTo(stream);
                }

                Blog item = new Blog()
                {
                    Title = blog.Title,
                    Content = blog.Content,
                    Image = newFileName,
                    PublishedDate = DateTime.Now,
                    AuthorId = blog.AuthorId,

                };

                
                _context.Add(item);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Id", blog.AuthorId);
            return View(blog);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Category ile birlikte proje verisini al
            var project = await _context.Blogs.Include(p => p.Author).FirstOrDefaultAsync(p => p.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            // Kategorileri yükle
            ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", project.AuthorId);

            return View(project);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Blog blog)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }


            //if (!ModelState.IsValid)
            //{
            //    ViewData["AuthorId"] = new SelectList(_context.Authors, "Id", "Name", blog.AuthorId);
            //    return View(blog);
            //}

            try
            {
                var existingBlog = await _context.Blogs.FindAsync(id);
                if (existingBlog == null)
                {
                    return NotFound();
                }
                string newFileName = existingBlog.Image;
                if (blog.ImageFile != null)
                {

                    if (!string.IsNullOrEmpty(existingBlog.Image))
                    {
                        string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogImages", existingBlog.Image);
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(blog.ImageFile.FileName);
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "BlogImages");


                    if (!Directory.Exists(wwwrootPath))
                    {
                        Directory.CreateDirectory(wwwrootPath);
                    }

                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);


                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await blog.ImageFile.CopyToAsync(stream);
                    }
                }


                existingBlog.Title = blog.Title;
                existingBlog.Content = blog.Content;
                existingBlog.Image = newFileName;
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
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
