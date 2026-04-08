using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;

namespace Dabbasheth.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet] public IActionResult Login() => View();

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
                TempData["UserEmail"] = user.Email;
                TempData["UserName"] = user.FullName;
                TempData["UserRole"] = user.Role;
                TempData.Keep();

                return user.Role == "Admin"
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Home");
            }

            TempData["Error"] = "Invalid credentials.";
            return View();
        }

        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                if (_context.Users.Any(u => u.Email.ToLower() == model.Email.ToLower()))
                {
                    TempData["Error"] = "Email already exists!";
                    return View(model);
                }

                model.Role = "Customer";
                model.Email = model.Email.Trim().ToLower();

                _context.Users.Add(model);

                _context.Wallets.Add(new Wallet
                {
                    UserEmail = model.Email,
                    Balance = 0,
                    Currency = "NGN",
                    CreatedAt = DateTime.UtcNow
                });

                _context.SaveChanges();

                TempData["Message"] = "Account created successfully! Login now.";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                TempData["Error"] = "Registration failed. Try again.";
                return View(model);
            }
        }

        public IActionResult Logout()
        {
            TempData.Clear();
            return RedirectToAction("Login");
        }
    }
}