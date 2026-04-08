using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;

namespace Dabbasheth.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ─── DASHBOARD ───────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Index()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

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

        // ─── CREDIT ACTION ───────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditAccount(string targetEmail, decimal amount,
                                           string accountType, int? planId)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            if (amount <= 0)
            {
                TempData["Error"] = "Amount must be greater than zero.";
                return RedirectToAction("Index");
            }

            if (accountType == "Wallet")
            {
                var wallet = _context.Wallets
                    .FirstOrDefault(w => w.UserEmail == targetEmail);

                if (wallet != null)
                {
                    wallet.Balance += amount;
                    AddAdminLog(targetEmail, amount, "Credit: Wallet");
                }
                else
                {
                    // Create wallet if it somehow doesn't exist yet
                    _context.Wallets.Add(new Wallet
                    {
                        UserEmail = targetEmail,
                        Balance = amount,
                        Currency = "NGN",
                        CreatedAt = DateTime.UtcNow
                    });
                    AddAdminLog(targetEmail, amount, "Credit: New Wallet Created");
                }
            }
            else if (accountType == "Thrift" && planId.HasValue)
            {
                var plan = _context.ThriftPlans
                    .FirstOrDefault(p => p.Id == planId.Value);

                if (plan != null)
                {
                    plan.CurrentSavings += amount;
                    AddAdminLog(targetEmail, amount, $"Credit Thrift: {plan.Title}");
                }
            }

            _context.SaveChanges();

            TempData["Message"] = "Account Credited Successfully!";
            return RedirectToAction("Index");
        }

        // ─── PRIVATE HELPER ──────────────────────────────────────────────────

        private void AddAdminLog(string targetEmail, decimal amount, string note)
        {
            var adminName = TempData.Peek("UserName")?.ToString() ?? "System";

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "ADM-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = targetEmail,
                Amount = amount,
                Description = $"{note} (By {adminName})",
                Date = DateTime.UtcNow,
                Status = "Approved"
            });
        }
    }
}