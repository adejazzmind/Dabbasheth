using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Dabbasheth.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context) => _context = context;

        // ── LOGIN ──────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() { HttpContext.Session.Clear(); TempData.Clear(); return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            { TempData["Error"] = "Email and password are required."; return RedirectToAction("Login"); }

            var clean = email.Trim().ToLower();

            // Emergency hard-coded bypass so CEO can always get in
            if (clean == "adejazzmind@gmail.com" && password == "123")
            {
                TempData["UserEmail"] = clean;
                TempData["UserName"] = "CEO Samson";
                TempData["UserRole"] = "Admin";
                TempData.Keep();
                return RedirectToAction("Index", "Admin");
            }

            try
            {
                var user = await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == clean && u.Password == password);
                if (user != null)
                {
                    if (user.Status == "Suspended" || user.Status == "Frozen")
                    { TempData["Error"] = "Account frozen. Contact support."; return RedirectToAction("Login"); }
                    TempData["UserEmail"] = user.Email;
                    TempData["UserName"] = user.FullName;
                    TempData["UserRole"] = user.Role;
                    TempData.Keep();
                    return user.Role == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Home");
                }
                TempData["Error"] = "Invalid credentials.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "DB Error: " + ex.Message;
                return RedirectToAction("Login");
            }
        }

        // ── REGISTER ───────────────────────────────────────────────────
        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            { TempData["Error"] = "All fields are required."; return RedirectToAction("Register"); }
            try
            {
                var clean = email.Trim().ToLower();
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == clean))
                { TempData["Error"] = "Email already registered."; return RedirectToAction("Login"); }
                _context.Users.Add(new User { FullName = fullName.Trim(), Email = clean, Password = password, Role = "Customer", Status = "Active", IsVerified = false, CreatedAt = DateTime.UtcNow });
                _context.Wallets.Add(new Wallet { UserEmail = clean, Balance = 0m, Currency = "NGN", WalletNumber = "DAB-" + new Random().Next(10000000, 99999999), CreatedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();
                TempData["Message"] = "Wallet created! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; return RedirectToAction("Register"); }
        }

        // ── LOGOUT / PROFILE ───────────────────────────────────────────
        public IActionResult Logout() { HttpContext.Session.Clear(); TempData.Clear(); return RedirectToAction("Login"); }

        public IActionResult Profile()
        {
            var email = TempData.Peek("UserEmail")?.ToString();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");
            ViewBag.UserEmail = email;
            return View();
        }
    }
}
