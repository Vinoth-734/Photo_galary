using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotoGalleryApp.Data;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.Controllers
{
    public class PhotoController : Controller
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PhotoController(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Photo
        public async Task<IActionResult> Index(string? search, string? tag)
        {
            var photosQuery = _db.Photos
                .Include(p => p.UploadedBy)
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.UploadedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                photosQuery = photosQuery.Where(p =>
                    p.Title!.Contains(search) ||
                    (p.Description != null && p.Description.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                // normalize: tag search as substring in comma-separated tags
                photosQuery = photosQuery.Where(p => p.Tags != null && p.Tags.Contains(tag));
            }

            var photos = await photosQuery.ToListAsync();
            ViewBag.Search = search ?? "";
            ViewBag.Tag = tag ?? "";
            ViewBag.CurrentRole = HttpContext.Items["Role"]?.ToString() ?? "";
            ViewBag.CurrentUserId = (int)(HttpContext.Items["UserId"] ?? 0);
            return View(photos);
        }

        public IActionResult Upload()
        {
            ViewBag.Role = HttpContext.Items["Role"]?.ToString() ?? "";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile? file, string title, string? description, string? tags)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Please select a file to upload.");
                return View();
            }

            if (string.IsNullOrWhiteSpace(title))
            {
                ModelState.AddModelError(nameof(title), "Title is required.");
                return View();
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var userId = HttpContext.Items["UserId"] as int? ?? 0;
            if (userId == 0)
            {
                // For convenience: create an anonymous user if not logged in
                var guest = new ApplicationUser { Username = "Guest", Role = "User" };
                _db.ApplicationUsers.Add(guest);
                await _db.SaveChangesAsync();
                userId = guest.Id;
                HttpContext.Session.SetInt32("UserId", userId);
            }

            var photo = new Photo
            {
                Title = title,
                Description = description,
                Tags = tags,
                FilePath = $"/uploads/{uniqueFileName}",
                UploadedAt = DateTime.UtcNow,
                UploadedById = userId
            };

            _db.Photos.Add(photo);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var role = HttpContext.Items["Role"]?.ToString() ?? "";
            if (role != "Admin")
            {
                return Unauthorized("Only Admins can delete photos.");
            }

            var photo = await _db.Photos.FindAsync(id);
            if (photo == null) return NotFound();

            // soft delete
            photo.IsDeleted = true;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Optional: show details
        public async Task<IActionResult> Details(int id)
        {
            var photo = await _db.Photos.Include(p => p.UploadedBy).FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
            if (photo == null) return NotFound();
            return View(photo);
        }
    }
}
