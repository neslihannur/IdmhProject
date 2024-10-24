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

        public async Task<IActionResult> Index()
        {
            return View(await _context.TeamMembers.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teamMember == null)
            {
                return NotFound();
            }

            return View(teamMember);
        }

       
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Position,Bio,Image,ImageFile")] TeamMember teamMember)
        {
            // Model geçerliliğini kontrol et
            if (!ModelState.IsValid)
            {
                // Resim dosyası var mı kontrol et
                if (teamMember.ImageFile != null)
                {
                    // Dosya yükleme işlemleri
                    string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip");

                    // Eğer dizin yoksa oluştur
                    if (!Directory.Exists(wwwrootPath))
                    {
                        Directory.CreateDirectory(wwwrootPath);
                    }

                    // Yeni dosya adını oluştur
                    string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(teamMember.ImageFile.FileName);
                    string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(imageFullPath, FileMode.Create))
                    {
                        await teamMember.ImageFile.CopyToAsync(stream);
                    }

                    // Modeldeki Image alanını güncelle
                    teamMember.Image = newFileName;
                }

                // Veritabanına kaydet
                _context.Add(teamMember);
                await _context.SaveChangesAsync();

                // Başarıyla ekledikten sonra listeye yönlendir
                return RedirectToAction(nameof(Index));
            }

            // Model geçerli değilse, tekrar formu göster
            return View(teamMember);
        }



        public async Task<IActionResult> Edit(int? id)
        {
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
            if (id != teamMember.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Veritabanından mevcut kaydı al
                    var existingMember = await _context.TeamMembers.FindAsync(id);
                    if (existingMember == null)
                    {
                        return NotFound();
                    }

                    // Resim dosyası var mı kontrol et
                    if (teamMember.ImageFile != null)
                    {
                        // Mevcut resim adını sakla
                        string existingImage = existingMember.Image;

                        // Eski resimleri sil
                        if (!string.IsNullOrEmpty(existingImage))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip", existingImage);
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        // Yeni dosya yükleme işlemleri
                        string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Ekip");

                        // Eğer dizin yoksa oluştur
                        if (!Directory.Exists(wwwrootPath))
                        {
                            Directory.CreateDirectory(wwwrootPath);
                        }

                        // Yeni dosya adını oluştur
                        string newFileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(teamMember.ImageFile.FileName);
                        string imageFullPath = Path.Combine(wwwrootPath, newFileName);

                        // Dosyayı kaydet
                        using (var stream = new FileStream(imageFullPath, FileMode.Create))
                        {
                            await teamMember.ImageFile.CopyToAsync(stream);
                        }

                        // Mevcut kaydın Image alanını güncelle
                        existingMember.Image = newFileName;
                    }

                    // Diğer alanları güncelle
                    existingMember.Name = teamMember.Name;
                    existingMember.Position = teamMember.Position;
                    existingMember.Bio = teamMember.Bio;

                    // Mevcut kaydı güncelle
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

            // Model geçerli değilse, tekrar formu göster
            return View(teamMember);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var teamMember = await _context.TeamMembers
                .FirstOrDefaultAsync(m => m.Id == id);
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
            var teamMember = await _context.TeamMembers.FindAsync(id);
            if (teamMember != null)
            {
                _context.TeamMembers.Remove(teamMember);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TeamMemberExists(int id)
        {
            return _context.TeamMembers.Any(e => e.Id == id);
        }

        public IActionResult Career()
        {

            var careers = _context.Career.ToList();


            if (careers == null || !careers.Any())
            {
                return View("Career");
            }


            return View(careers);
        }
        // GET: Career/Create
        public IActionResult CareerCreate()
        {
            return View();
        }

        // POST: Career/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CareerCreate([Bind("Id,Profile,Award")] Career career)
        {
            if (ModelState.IsValid)
            {
                _context.Add(career);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Career)); // Başarılı bir ekleme sonrası yönlendirme
            }
            return View(career);
        }
        [HttpPost]
        public IActionResult DeleteCareer(int id)
        {
            // Silinecek kariyeri veritabanında bul
            var career = _context.Career.FirstOrDefault(c => c.Id == id);

            // Eğer kariyer bulunamadıysa, hata döndür
            if (career == null)
            {
                return NotFound();
            }

            // Kariyeri veritabanından sil
            _context.Career.Remove(career);

            // Değişiklikleri kaydet
            _context.SaveChanges();

            // İşlem başarılıysa anasayfaya yönlendir
            return RedirectToAction("Career");
        }



    }
}
