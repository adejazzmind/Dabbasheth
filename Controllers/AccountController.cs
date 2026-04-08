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

        // --- LOGIN GET ---
        [HttpGet]
        public IActionResult Login() => View();

        // --- LOGIN POST ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            string cleanEmail = email?.Trim().ToLower();

            // Search the Neon database for the user
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

        // --- REGISTER GET ---
        [HttpGet]
        public IActionResult Register() => View();

        // --- REGISTER POST (Paystack-Free Version) ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            // 1. Check if user already exists
            if (_context.Users.Any(u => u.Email == model.Email))
            {
                TempData["Error"] = "Email already exists!";
                return View(model);
            }

            // 2. Set default customer role
            model.Role = "Customer";

            // 3. Stage the User for Neon
            _context.Users.Add(model);

            // 4. Create the Wallet automatically (Starts at 0 balance)
            _context.Wallets.Add(new Wallet
            {
                UserEmail = model.Email,
                Balance = 0,
                CreatedAt = DateTime.UtcNow
            });

            // 5. Single push to Neon database
            _context.SaveChanges();

            TempData["Message"] = "Account Created Successfully!";

            // 6. Direct redirect to login (skipping payment screens)
            return RedirectToAction("Login");
        }

        // --- LOGOUT ---
        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}