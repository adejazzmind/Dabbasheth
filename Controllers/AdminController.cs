using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System.Linq;

namespace Dabbasheth.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            // Force debug output
            System.Diagnostics.Debug.WriteLine("=== ADMIN PAGE LOADED ===");
            Console.WriteLine("=== ADMIN PAGE LOADED ===");
            Console.WriteLine($"Users Count: {_context.Users.Count()}");
            Console.WriteLine($"Wallets Count: {_context.Wallets.Count()}");
            Console.WriteLine($"Total Balance: {_context.Wallets.Sum(w => (decimal?)w.Balance) ?? 0}");

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(u => u.Role == "Customer"),
                TotalSystemBalance = _context.Wallets.Sum(w => (decimal?)w.Balance) ?? 0m,
                AllUsers = _context.Users.ToList(),
                AllWallets = _context.Wallets.ToList(),
                AllThriftPlans = _context.ThriftPlans.ToList()
            };

            return View(viewModel);
        }
    }
}