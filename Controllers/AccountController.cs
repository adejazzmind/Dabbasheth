using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;

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
        // LOGIN - GET
        // ────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Login() => View();

        // ────────────────────────────────────────────────────────────────
        // LOGIN - POST
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

            try
            {
                string cleanEmail = email.Trim().ToLower();

                var user = _context.Users.FirstOrDefault(u =>
                    u.Email.ToLower() == cleanEmail && u.Password == password);

                if (user != null)
                {
                    TempData["UserEmail"] = user.Email;
                    TempData["UserName"] = user.FullName;
                    TempData["UserRole"] = user.Role;
                    TempData.Keep();

                    return user.Role == "Admin"
                        ? RedirectToAction("Index", "Admin")
                        : RedirectToAction("Index", "Home");
                }

                TempData["Error"] = "Invalid email or password.";
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login Error: {ex.Message}");
                TempData["Error"] = "Unable to connect to the server. Please try again later.";
                return View();
            }
        }

        // ────────────────────────────────────────────────────────────────
        // REGISTER - GET
        // ────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Register() => View();

        // ────────────────────────────────────────────────────────────────
        // REGISTER - POST (Join Hub)
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
                string cleanEmail = model.Email.Trim().ToLower();

                // Check for duplicate email
                if (_context.Users.Any(u => u.Email.ToLower() == cleanEmail))
                {
                    TempData["Error"] = "An account with this email already exists!";
                    return View(model);
                }

                // Prepare user
                model.Email = cleanEmail;
                model.Role = "Customer";

                _context.Users.Add(model);

                // Create wallet automatically
                _context.Wallets.Add(new Wallet
                {
                    UserEmail = cleanEmail,
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
                Console.WriteLine($"Register Error: {ex.Message}");
                TempData["Error"] = "Failed to create account. Please try again later.";
                return View(model);
            }
        }

        // ────────────────────────────────────────────────────────────────
        // LOGOUT
        // ────────────────────────────────────────────────────────────────
        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}