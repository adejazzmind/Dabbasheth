using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. DASHBOARD - CONSOLIDATED CEO VIEW
        // ==========================================
        [HttpGet]
        public IActionResult Index()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin")
            {
                return RedirectToAction("Login", "Account");
            }

            // 🚀 FINANCIAL AGGREGATION ENGINE
            // Summing both Liquid Wallets and Target Savings for true system liquidity
            var totalWalletBalance = _context.Wallets.Sum(w => (decimal?)w.Balance) ?? 0m;
            var totalThriftBalance = _context.ThriftPlans.Sum(p => (decimal?)p.CurrentSavings) ?? 0m;

            var grandTotalLiquidity = totalWalletBalance + totalThriftBalance;

            // Operational Logs for MD Verification
            Console.WriteLine($"--- Admin Audit: {DateTime.Now} ---");
            Console.WriteLine($"Liquid Cash: ₦{totalWalletBalance:N2}");
            Console.WriteLine($"Thrift Assets: ₦{totalThriftBalance:N2}");
            Console.WriteLine($"Total Assets: ₦{grandTotalLiquidity:N2}");

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = _context.Users.Count(u => u.Role == "Customer"),

                // This reflects the absolute total liquidity of IES Life Hub
                TotalSystemBalance = grandTotalLiquidity,

                AllUsers = _context.Users.ToList(),
                AllWallets = _context.Wallets.ToList(),
                AllThriftPlans = _context.ThriftPlans.ToList()
            };

            return View(viewModel);
        }

        // ==========================================
        // 2. OPERATIONS - ACCOUNT CREDITING
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditAccount(string targetEmail, decimal amount, string accountType)
        {
            if (string.IsNullOrEmpty(targetEmail) || amount <= 0)
            {
                TempData["Error"] = "Invalid credit request. Please check the amount and recipient.";
                return RedirectToAction("Index");
            }

            try
            {
                string cleanEmail = targetEmail.Trim().ToLower();

                if (accountType == "Wallet")
                {
                    var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == cleanEmail);
                    if (wallet != null)
                    {
                        wallet.Balance += amount;
                    }
                    else
                    {
                        throw new Exception("Wallet not found for this user.");
                    }
                }
                else if (accountType == "Thrift")
                {
                    // Targets the most recently created active Thrift goal
                    var plan = _context.ThriftPlans
                        .Where(p => p.UserEmail.ToLower() == cleanEmail && p.Status == "Active")
                        .OrderByDescending(p => p.StartDate)
                        .FirstOrDefault();

                    if (plan != null)
                    {
                        plan.CurrentSavings += amount;
                    }
                    else
                    {
                        throw new Exception("No active Thrift Plan found for this user.");
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Successfully credited {accountType} with ₦{amount:N2}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Credit Failed: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}