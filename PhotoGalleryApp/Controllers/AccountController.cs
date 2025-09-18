using Microsoft.AspNetCore.Mvc;
using PhotoGalleryApp.Data;
using PhotoGalleryApp.Models;
using Microsoft.EntityFrameworkCore;

namespace PhotoGalleryApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;
        public AccountController(AppDbContext db) => _db = db;

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string role)
        {
            if (string.IsNullOrWhiteSpace(username)) return View();

            role = (role == "Admin") ? "Admin" : "User";

            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                user = new ApplicationUser { Username = username, Role = role };
                _db.ApplicationUsers.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                // update role (for dev/testing)
                user.Role = role;
                await _db.SaveChangesAsync();
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            TempData["Message"] = $"Logged in as {user.Username} ({user.Role})";

            return RedirectToAction("Index", "Photo");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Photo");
        }
    }
}
