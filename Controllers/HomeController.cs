using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Linq;
using System.Diagnostics;

namespace Dabbasheth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────────────────────
        // Helper: Get Current Logged-in User Email
        // ────────────────────────────────────────────────────────────────
        private string? GetLoggedInUserEmail()
        {
            var email = TempData.Peek("UserEmail") as string;
            return string.IsNullOrEmpty(email) ? null : email.ToLower();
        }

        // ────────────────────────────────────────────────────────────────
        // DASHBOARD
        // ────────────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email);
            var transactions = _context.Transactions
                .Where(t => t.UserEmail.ToLower() == email)
                .OrderByDescending(t => t.Date)
                .ToList();

            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            ViewBag.UserEmail = email;

            return View(transactions);
        }

        // ────────────────────────────────────────────────────────────────
        // THRIFT / WEALTH GOALS
        // ────────────────────────────────────────────────────────────────
        public IActionResult Thrift()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var plans = _context.ThriftPlans
                .Where(p => p.UserEmail.ToLower() == email && p.Status == "Active")
                .ToList();

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View(plans);
        }

        // ────────────────────────────────────────────────────────────────
        // WITHDRAW
        // ────────────────────────────────────────────────────────────────
        public IActionResult Withdraw()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // PAY BILLS
        // ────────────────────────────────────────────────────────────────
        public IActionResult PayBills()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // LOAN
        // ────────────────────────────────────────────────────────────────
        public IActionResult Loan()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            ViewBag.UserEmail = email;
            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // REWARDS
        // ────────────────────────────────────────────────────────────────
        public IActionResult Rewards()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            ViewBag.Cashback = 36.50m;
            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // PROFILE
        // ────────────────────────────────────────────────────────────────
        public IActionResult Profile()
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login", "Account");

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail.ToLower() == email);
            ViewBag.UserEmail = email;
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View();
        }

        // ────────────────────────────────────────────────────────────────
        // POST ACTIONS
        // ────────────────────────────────────────────────────────────────

        [HttpPost]
        public IActionResult ProcessLoan(decimal loanAmount)
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || loanAmount <= 0)
                return RedirectToAction("Index");

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "LOAN-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = email,
                Amount = loanAmount,
                Description = $"IES Loan Application: ₦{loanAmount:N2}",
                Date = DateTime.UtcNow,
                Status = "Pending Approval"
            });

            _context.SaveChanges();
            TempData["Message"] = "Loan application submitted! Admin will review shortly.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || amount <= 0)
                return RedirectToAction("Index");

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "WTH-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = email,
                Amount = amount * -1,
                Description = $"Withdrawal to {bankName} ({accountNumber})",
                Date = DateTime.UtcNow,
                Status = "Pending Approval"
            });

            _context.SaveChanges();
            TempData["Message"] = "Withdrawal request sent!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessBillPayment(decimal amount, string provider, string phoneNumber)
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || amount <= 0)
                return RedirectToAction("Index");

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "BILL-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = email,
                Amount = amount * -1,
                Description = $"{provider} Airtime: {phoneNumber}",
                Date = DateTime.UtcNow,
                Status = "Success"
            });

            _context.SaveChanges();
            TempData["Message"] = "Payment Successful!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateThriftPlan(string title, decimal targetAmount, string frequency, int durationMonths)
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Thrift");

            _context.ThriftPlans.Add(new ThriftPlan
            {
                UserEmail = email,
                Title = title,
                TargetAmount = targetAmount,
                CurrentSavings = 0,
                Frequency = frequency,
                StartDate = DateTime.UtcNow,
                MaturityDate = DateTime.UtcNow.AddMonths(durationMonths),
                Status = "Active"
            });

            _context.SaveChanges();
            TempData["Message"] = "Goal Created Successfully!";
            return RedirectToAction("Thrift");
        }

        [HttpPost]
        public IActionResult SaveToThrift(int planId, decimal amount)
        {
            string? email = GetLoggedInUserEmail();
            if (string.IsNullOrEmpty(email) || amount <= 0)
                return RedirectToAction("Thrift");

            var plan = _context.ThriftPlans.FirstOrDefault(p => p.Id == planId);
            if (plan != null && plan.UserEmail.ToLower() == email)
            {
                plan.CurrentSavings += amount;

                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = "SAV-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                    UserEmail = email,
                    Amount = amount * -1,
                    Description = $"Savings: {plan.Title}",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                _context.SaveChanges();
                TempData["Message"] = "Savings Updated!";
            }

            return RedirectToAction("Thrift");
        }

        // ────────────────────────────────────────────────────────────────
        // ERROR PAGE
        // ────────────────────────────────────────────────────────────────
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}