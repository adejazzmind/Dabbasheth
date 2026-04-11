using Dabbasheth.Data;
using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
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

        // --- 1. CORE VIEWS ---

        public IActionResult Index()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email.ToLower());
            var transactions = _context.Transactions
                .Where(t => t.UserEmail.ToLower() == email.ToLower())
                .OrderByDescending(t => t.Date).ToList();

            ViewBag.Balance = wallet?.Balance ?? 0m;
            ViewBag.UserEmail = email;
            return View(transactions);
        }

        public IActionResult Profile()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            ViewBag.UserEmail = email;
            return View();
        }

        // --- 2. THRIFT ENGINE (THE LIVE CALCULATION AREA) ---

        public IActionResult Thrift()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var plans = _context.ThriftPlans
                .Where(p => p.UserEmail.ToLower() == email.ToLower()).ToList();

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
                    StartDate = DateTime.UtcNow
                };

                newPlan.CalculateMaturity();
                _context.ThriftPlans.Add(newPlan);
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Savings goal created!";
            }
            catch (Exception ex) { TempData["Error"] = "Error: " + ex.Message; }

            return RedirectToAction("Thrift");
        }

        // ✅ THE BULLETPROOF LIVE CALCULATION
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepositToThrift(int planId, decimal amount)
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            try
            {
                // 1. Fetch data with explicit tracking for accuracy
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
                var plan = await _context.ThriftPlans.FindAsync(planId);

                // 2. Validation Checks
                if (wallet == null || plan == null)
                {
                    TempData["Error"] = "Account data sync error.";
                    return RedirectToAction("Thrift");
                }
                if (amount <= 0)
                {
                    TempData["Error"] = "Please enter a valid amount.";
                    return RedirectToAction("Thrift");
                }

                // 3. Balance Check
                if (wallet.Balance < amount)
                {
                    TempData["Error"] = $"❌ Insufficient Funds. Your balance is ₦{wallet.Balance:N2}";
                    return RedirectToAction("Thrift");
                }

                // 4. THE LIVE MATH (Atomic Swap)
                wallet.Balance -= amount;
                plan.CurrentSavings += amount;

                // 5. UPDATE CONTEXT EXPLICITLY
                _context.Wallets.Update(wallet);
                _context.ThriftPlans.Update(plan);

                // 6. CREATE AUDIT TRAIL
                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = "SAV-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                    UserEmail = email,
                    Amount = amount,
                    Description = $"Thrift Savings: {plan.Title}",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                // Status Update if Milestone Reached
                if (plan.CurrentSavings >= plan.TargetAmount) plan.Status = "Completed";

                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ ₦" + amount.ToString("N2") + " saved successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Database Error: " + ex.Message;
            }

            return RedirectToAction("Thrift");
        }

        // --- 3. OTHER OPERATIONS ---
        public IActionResult Withdraw()
        {
            string email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email.ToLower());
            ViewBag.Balance = wallet?.Balance ?? 0m;
            return View();
        }

        public IActionResult PayBills() => View();
        public IActionResult Rewards() { ViewBag.Cashback = 36.00m; return View(); }
    }
}