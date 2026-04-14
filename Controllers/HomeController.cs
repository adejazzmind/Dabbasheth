using Dabbasheth.Data;
using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- 0. IDENTITY HELPER ---
        private string GetLoggedInUserEmail() => TempData.Peek("UserEmail") as string;

        // ==========================================
        // 1. CORE HUB VIEWS
        // ==========================================

        public async Task<IActionResult> Index()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            var transactions = await _context.Transactions
                .Where(t => t.UserEmail.ToLower() == email.ToLower())
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            ViewBag.Balance = wallet?.Balance ?? 0m;
            ViewBag.UserEmail = email;
            ViewBag.WalletNumber = wallet?.WalletNumber ?? "N/A";

            return View(transactions);
        }

        public IActionResult Profile()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            ViewBag.UserEmail = email;
            return View();
        }

        // ==========================================
        // 2. WITHDRAWAL OPERATIONS (CEO APPROVAL FLOW)
        // ==========================================

        public async Task<IActionResult> Withdraw()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            ViewBag.Balance = wallet?.Balance ?? 0m;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());

            if (wallet == null || wallet.Balance < amount || amount <= 0)
            {
                TempData["Error"] = "❌ Invalid withdrawal request or insufficient funds.";
                return RedirectToAction("Withdraw");
            }

            var request = new TransactionRecord
            {
                Reference = "WTH-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = email,
                Amount = amount,
                Description = $"Withdrawal to {bankName} ({accountNumber})",
                Date = DateTime.UtcNow,
                Status = "Pending" // 🔒 CEO Approval required
            };

            _context.Transactions.Add(request);
            await _context.SaveChangesAsync();

            TempData["Message"] = "🚀 Request sent! Awaiting CEO Approval.";
            return RedirectToAction("Index");
        }

        // ==========================================
        // 3. THRIFT ENGINE (SAVINGS GOALS)
        // ==========================================

        public async Task<IActionResult> Thrift()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var plans = await _context.ThriftPlans
                .Where(p => p.UserEmail.ToLower() == email.ToLower())
                .ToListAsync();

            ViewBag.ThriftPlans = plans;
            ViewBag.TotalSavings = plans.Sum(p => (decimal?)p.CurrentSavings) ?? 0m;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftPlan(string title, decimal targetAmount, string frequency)
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || targetAmount <= 0)
            {
                TempData["Error"] = "Invalid goal parameters.";
                return RedirectToAction("Thrift");
            }

            try
            {
                var newPlan = new ThriftPlan
                {
                    Title = title.Trim(),
                    TargetAmount = targetAmount,
                    CurrentSavings = 0,
                    Frequency = frequency ?? "Daily",
                    UserEmail = email,
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    // MaturityDate is calculated inside the model's CalculateMaturity method
                    MaturityDate = DateTime.UtcNow.AddMonths(6)
                };

                // Use the internal model logic if available
                newPlan.CalculateMaturity();

                _context.ThriftPlans.Add(newPlan);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Savings goal created successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "System Error: " + ex.Message;
            }

            return RedirectToAction("Thrift");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepositToThrift(int planId, decimal amount)
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            try
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
                var plan = await _context.ThriftPlans.FindAsync(planId);

                if (wallet == null || plan == null || amount <= 0)
                {
                    TempData["Error"] = "Account synchronization error.";
                    return RedirectToAction("Thrift");
                }

                if (wallet.Balance < amount)
                {
                    TempData["Error"] = $"❌ Insufficient Funds. Your balance is ₦{wallet.Balance:N2}";
                    return RedirectToAction("Thrift");
                }

                // 🏦 ATOMIC SWAP: Money moves from Wallet to Thrift
                wallet.Balance -= amount;
                plan.CurrentSavings += amount;

                _context.Wallets.Update(wallet);
                _context.ThriftPlans.Update(plan);

                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = "SAV-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                    UserEmail = email,
                    Amount = amount,
                    Description = $"Thrift Savings: {plan.Title}",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                if (plan.CurrentSavings >= plan.TargetAmount) plan.Status = "Completed";

                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ ₦{amount:N2} moved to your savings goal!";
            }
            catch (Exception ex) { TempData["Error"] = "Database Error: " + ex.Message; }

            return RedirectToAction("Thrift");
        }

        // ==========================================
        // 4. UTILITIES
        // ==========================================
        public IActionResult PayBills() => View();
        public IActionResult Rewards() { ViewBag.Cashback = 36.00m; return View(); }
    }
}