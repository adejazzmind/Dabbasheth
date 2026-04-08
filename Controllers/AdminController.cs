using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System.Linq;
using System;

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
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(u => u.Role == "Customer"),
                TotalSystemBalance = _context.Wallets.Sum(w => w.Balance),
                AllUsers = _context.Users.ToList(),
                AllWallets = _context.Wallets.ToList(),
                AllThriftPlans = _context.ThriftPlans.ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditAccount(string targetEmail, decimal amount, string accountType)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == targetEmail);
            if (wallet != null)
            {
                wallet.Balance += amount;

                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = "ADM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    UserEmail = targetEmail,
                    Amount = amount,
                    Description = "Admin Credit (Verified)",
                    Date = DateTime.UtcNow,
                    Status = "Approved"
                });

                _context.SaveChanges();
                TempData["Message"] = "Account Credited!";
            }

            return RedirectToAction("Index");
        }
    }
}