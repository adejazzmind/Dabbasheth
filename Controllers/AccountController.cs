using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System.Linq;

namespace Dabbasheth.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────
        // GET: Login Page
        // ────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        // ────────────────────────────────────────────────────────────────
        // POST: Login User
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Email and password are required.";
                return View();
            }

            string cleanEmail = email.Trim().ToLower();

            var user = _context.Users.FirstOrDefault(u =>
                u.Email.ToLower() == cleanEmail && u.Password == password);

            if (user != null)
            {
                // Store user info in TempData for this session
                TempData["UserEmail"] = user.Email;
                TempData["UserName"] = user.FullName;
                TempData["UserRole"] = user.Role;
                TempData.Keep();

                // Redirect based on role
                return user.Role == "Admin"
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Invalid email or password.";
            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // GET: Register Page
        // ────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        // ────────────────────────────────────────────────────────────────
        // POST: Register New User (Improved Version)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Check for existing email (case-insensitive)
                if (_context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                {
                    TempData["Error"] = "An account with this email already exists!";
                    return View(model);
                }

                // Set default values
                model.Role = "Customer";
                model.Email = model.Email.Trim().ToLower();   // Normalize email

                // Add User
                _context.Users.Add(model);

                // Create Wallet automatically
                _context.Wallets.Add(new Wallet
                {
                    UserEmail = model.Email,
                    Balance = 0,
                    Currency = "NGN",
                    CreatedAt = DateTime.UtcNow
                });

                _context.SaveChanges();

                TempData["Message"] = "Account created successfully! You can now login.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // You can add proper logging here later
                TempData["Error"] = "Registration failed. Please try again later.";
                return View(model);
            }
        }

        // ────────────────────────────────────────────────────────────────
        // Logout
        // ────────────────────────────────────────────────────────────────
        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}