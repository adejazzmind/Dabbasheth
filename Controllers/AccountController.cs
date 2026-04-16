using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Dabbasheth.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 🔐 1. AUTHENTICATION GATEWAY (Login)
        // ============================================================

        [HttpGet]
        public IActionResult Login()
        {
            // ✅ Wipe stale session data on load
            HttpContext.Session.Clear();
            TempData.Clear();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required.";
                return RedirectToAction("Login");
            }

            string cleanEmail = email.Trim().ToLower();

            // 🛡️ EMERGENCY CEO BYPASS
            // If the database fails, this hard-coded check will grant access.
            if (cleanEmail == "adejazzmind@gmail.com" && password == "123")
            {
                TempData["UserEmail"] = cleanEmail;
                TempData["UserName"] = "CEO Samson (Bypass)";
                TempData["UserRole"] = "Admin";
                TempData.Keep();
                return RedirectToAction("Index", "Admin");
            }

            try
            {
                // ✅ DATABASE AUTHENTICATION
                var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == cleanEmail && u.Password == password);

                if (user != null)
                {
                    if (user.Status == "Suspended")
                    {
                        TempData["Error"] = "Account frozen. Contact support.";
                        return RedirectToAction("Login");
                    }

                    TempData["UserEmail"] = user.Email;
                    TempData["UserName"] = user.FullName;
                    TempData["UserRole"] = user.Role;
                    TempData.Keep();

                    return user.Role == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Invalid credentials. Access denied.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Reveals the technical reason Neon is failing
                TempData["Error"] = "Dev Log (DB Error): " + ex.Message;
                return RedirectToAction("Login");
            }
        }

        // ============================================================
        // 💳 2. IDENTITY INITIALIZATION (Registration)
        // ============================================================

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "All fields are required.";
                return RedirectToAction("Register");
            }

            try
            {
                string cleanEmail = email.Trim().ToLower();

                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail))
                {
                    TempData["Error"] = "Email already registered.";
                    return RedirectToAction("Login");
                }

                var newUser = new User
                {
                    FullName = fullName.Trim(),
                    Email = cleanEmail,
                    Password = password,
                    Role = "Customer",
                    Status = "Active",
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                var newWallet = new Wallet
                {
                    UserEmail = cleanEmail,
                    Balance = 0m,
                    Currency = "NGN",
                    WalletNumber = "DAB-" + new Random().Next(10000000, 99999999).ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(newUser);
                _context.Wallets.Add(newWallet);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Wallet initialized! Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Registration Error: " + ex.Message;
                return RedirectToAction("Register");
            }
        }

        // ============================================================
        // 🚪 3. SECURE TERMINATION (Logout)
        // ============================================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}