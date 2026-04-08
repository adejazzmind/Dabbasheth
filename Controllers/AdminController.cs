using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
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

        // ────────────────────────────────────────────────────────────────
        // ADMIN DASHBOARD
        // ────────────────────────────────────────────────────────────────
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

        // ────────────────────────────────────────────────────────────────
        // CREDIT ACCOUNT (God Mode)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditAccount(string targetEmail, decimal amount,
                                           string accountType, int? planId)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin")
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(targetEmail) || amount <= 0)
            {
                TempData["Error"] = "Invalid email or amount.";
                return RedirectToAction("Index");
            }

            try
            {
                if (accountType == "Wallet")
                {
                    var wallet = _context.Wallets
                        .FirstOrDefault(w => w.UserEmail.ToLower() == targetEmail.ToLower());

                    if (wallet != null)
                    {
                        wallet.Balance += amount;
                        AddAdminLog(targetEmail, amount, "Credit: Wallet");
                    }
                    else
                    {
                        // Create wallet if it doesn't exist
                        _context.Wallets.Add(new Wallet
                        {
                            UserEmail = targetEmail.ToLower(),
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
                    else
                    {
                        TempData["Error"] = "Thrift plan not found.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    TempData["Error"] = "Invalid account type selected.";
                    return RedirectToAction("Index");
                }

                _context.SaveChanges();
                TempData["Message"] = $"₦{amount:N2} successfully credited to {targetEmail}!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to credit account. Please try again.";
                // TODO: Add proper logging in production
                Console.WriteLine($"Credit Error: {ex.Message}");
            }

            return RedirectToAction("Index");
        }

        // ────────────────────────────────────────────────────────────────
        // PRIVATE HELPER: Add Admin Log
        // ────────────────────────────────────────────────────────────────
        private void AddAdminLog(string targetEmail, decimal amount, string note)
        {
            var adminName = TempData.Peek("UserName")?.ToString() ?? "System Admin";

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "ADM-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = targetEmail.ToLower(),
                Amount = amount,
                Description = $"{note} (By {adminName})",
                Date = DateTime.UtcNow,
                Status = "Approved"
            });
        }
    }
}