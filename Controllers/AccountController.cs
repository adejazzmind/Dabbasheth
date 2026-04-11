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

        // ==========================================
        // 1. LOGIN SYSTEM
        // ==========================================

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required.";
                return View();
            }

            try
            {
                string cleanEmail = email.Trim().ToLower();

                // Check database for user
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == cleanEmail && u.Password == password);

                if (user != null)
                {
                    // Set Session State
                    TempData["UserEmail"] = user.Email;
                    TempData["UserName"] = user.FullName;
                    TempData["UserRole"] = user.Role;
                    TempData.Keep();

                    // Routing Logic for MD/CEO vs Customers
                    return user.Role == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Invalid email or password.";
                return View();
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Authentication server error. Please try again.";
                return View();
            }
        }

        // ==========================================
        // 2. REGISTRATION SYSTEM (Growth Engine)
        // ==========================================

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "All fields are required to join the Hub.";
                return RedirectToAction("Login");
            }

            try
            {
                string cleanEmail = email.Trim().ToLower();

                // 🛑 BLOCK: Duplicate Emails
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == cleanEmail))
                {
                    TempData["Error"] = "This email is already linked to a Dabbasheth wallet.";
                    return RedirectToAction("Login");
                }

                // ✅ STEP 1: Define the User Profile
                var newUser = new User
                {
                    FullName = fullName.Trim(),
                    Email = cleanEmail,
                    Password = password, // Security Note: Implement hashing later
                    Role = "Customer",
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ STEP 2: Define the Virtual Wallet
                var newWallet = new Wallet
                {
                    UserEmail = cleanEmail,
                    Balance = 0m,
                    Currency = "NGN",
                    WalletNumber = "DAB-" + new Random().Next(10000000, 99999999).ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                // ✅ STEP 3: Atomic Save to Database
                _context.Users.Add(newUser);
                _context.Wallets.Add(newWallet);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Welcome to IES Life Hub! Your wallet is ready. Please login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Registration Failed: " + ex.Message;
                return RedirectToAction("Login");
            }
        }

        // ==========================================
        // 3. SESSION TERMINATION
        // ==========================================

        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}