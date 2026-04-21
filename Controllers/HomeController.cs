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

        // --- 🛡️ IDENTITY HELPER ---
        private string GetLoggedInUserEmail() => TempData.Peek("UserEmail")?.ToString();

        // ============================================================
        // 📊 1. HUB DASHBOARD (Customer Experience)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var viewModel = new UserDashboardViewModel
            {
                Wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower()),

                ThriftPlans = await _context.ThriftPlans
                    .Where(p => p.UserEmail.ToLower() == email.ToLower())
                    .ToListAsync(),

                // ✅ SYNCHRONIZED: Using 'Transaction' class and 'Date' property
                RecentTransactions = await _context.Transactions
                    .Where(t => t.UserEmail.ToLower() == email.ToLower())
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToListAsync(),

                ThriftGroups = await _context.ThriftGroups
                    .Include(g => g.MemberPlans)
                    .Where(g => g.MemberPlans.Any(m => m.UserEmail.ToLower() == email.ToLower()))
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Profile()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            ViewBag.UserEmail = email;
            return View();
        }

        // ============================================================
        // 💸 2. TREASURY OPERATIONS (Withdrawals)
        // ============================================================
        [HttpGet]
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
                TempData["Error"] = "❌ Invalid request or insufficient funds.";
                return RedirectToAction("Withdraw");
            }

            // ✅ SYNCHRONIZED: Creating a standard 'Transaction' record
            var request = new Transaction
            {
                UserEmail = email,
                Amount = amount,
                Description = $"Withdrawal: {bankName} ({accountNumber})",
                Date = DateTime.UtcNow,
                Type = "Debit",
                Status = "Pending" // CEO/Admin approval required
            };

            _context.Transactions.Add(request);
            await _context.SaveChangesAsync();

            TempData["Message"] = "🚀 Withdrawal request logged! Awaiting Hub processing.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 💰 3. SAVINGS ENGINE (Individual Thrift)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Thrift()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var plans = await _context.ThriftPlans
                .Where(p => p.UserEmail.ToLower() == email.ToLower()).ToListAsync();

            ViewBag.ThriftPlans = plans;
            ViewBag.TotalSavings = plans.Sum(p => (decimal?)p.CurrentSavings) ?? 0m;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftPlan(string title, decimal targetAmount, string frequency)
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || targetAmount <= 0) return RedirectToAction("Thrift");

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
                    MaturityDate = DateTime.UtcNow.AddMonths(6)
                };

                _context.ThriftPlans.Add(newPlan);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Savings goal successfully initialized!";
            }
            catch (Exception ex) { TempData["Error"] = "Setup error: " + ex.Message; }

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

                if (wallet == null || plan == null || amount <= 0 || wallet.Balance < amount)
                {
                    TempData["Error"] = "❌ Fund synchronization failed or insufficient balance.";
                    return RedirectToAction("Thrift");
                }

                // 🏦 ATOMIC TRANSACTION: Wallet to Savings
                wallet.Balance -= amount;
                plan.CurrentSavings += amount;

                // ✅ SYNCHRONIZED: Creating a standard 'Transaction' record
                _context.Transactions.Add(new Transaction
                {
                    UserEmail = email,
                    Amount = amount,
                    Description = $"Savings Deposit: {plan.Title}",
                    Date = DateTime.UtcNow,
                    Type = "Debit",
                    Status = "Success"
                });

                if (plan.CurrentSavings >= plan.TargetAmount) plan.Status = "Completed";

                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ ₦{amount:N2} added to your goal!";
            }
            catch (Exception ex) { TempData["Error"] = "Processing error: " + ex.Message; }

            return RedirectToAction("Thrift");
        }

        // ============================================================
        // 🛠️ 4. UTILITIES
        // ============================================================
        public IActionResult PayBills() => View();

        public IActionResult Rewards()
        {
            ViewBag.Cashback = 36.00m;
            return View();
        }
    }
}