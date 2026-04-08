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

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            string cleanEmail = email?.Trim().ToLower();

            // Search the REAL Neon database for the user
            var user = _context.Users.FirstOrDefault(u => u.Email.ToLower() == cleanEmail && u.Password == password);

            if (user != null)
            {
                TempData["UserEmail"] = user.Email;
                TempData["UserName"] = user.FullName;
                TempData["UserRole"] = user.Role;
                TempData.Keep();

                return user.Role == "Admin" ? RedirectToAction("Index", "Admin") : RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Invalid credentials.";
            return View();
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                TempData["Error"] = "Email already exists!";
                return View();
            }

            model.Role = "Customer";
            _context.Users.Add(model);

            _context.Wallets.Add(new Wallet
            {
                UserEmail = model.Email,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            });

            _context.SaveChanges();
            TempData["Message"] = "Account Created!";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}