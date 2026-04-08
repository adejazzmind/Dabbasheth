using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // ─── UTILITIES ──────────────────────────────────────────────────────

        private string GetLoggedInUser()
        {
            // TempData.Peek keeps the value alive across redirects
            var email = TempData.Peek("UserEmail") as string;
            return string.IsNullOrEmpty(email) ? "adejazzmind@gmail.com" : email;
        }

        // ─── GET ACTIONS ─────────────────────────────────────────────────────

        public IActionResult Index()
        {
            string userEmail = GetLoggedInUser();

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);

            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            ViewBag.UserEmail = userEmail;

            var transactions = _context.Transactions
                .Where(t => t.UserEmail == userEmail)
                .OrderByDescending(t => t.Date)
                .ToList();

            return View(transactions);
        }

        public IActionResult Thrift()
        {
            string userEmail = GetLoggedInUser();

            var plans = _context.ThriftPlans
                .Where(p => p.UserEmail == userEmail && p.Status == "Active")
                .ToList();

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);

            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View(plans);
        }

        public IActionResult Withdraw()
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        public IActionResult PayBills()
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        public IActionResult Loan()
        {
            string userEmail = GetLoggedInUser();
            ViewBag.UserEmail = userEmail;
            return View();
        }

        public IActionResult Rewards()
        {
            ViewBag.Cashback = 36.50m;
            return View();
        }

        public IActionResult Profile()
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.UserEmail = userEmail;
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        // ─── POST ACTIONS (TRANSACTIONS) ─────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessLoan(decimal loanAmount)
        {
            string userEmail = GetLoggedInUser();

            if (loanAmount <= 0)
            {
                TempData["Error"] = "Please enter a valid loan amount.";
                return RedirectToAction("Loan");
            }

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "LOAN-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = userEmail,
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
        [ValidateAntiForgeryToken]
        public IActionResult ProcessWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            string userEmail = GetLoggedInUser();

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);

            if (wallet == null || wallet.Balance < amount)
            {
                TempData["Error"] = "Insufficient funds!";
                return RedirectToAction("Withdraw");
            }

            if (amount <= 0)
            {
                TempData["Error"] = "Please enter a valid amount.";
                return RedirectToAction("Withdraw");
            }

            // Deduct balance immediately on withdrawal request
            wallet.Balance -= amount;

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "WTH-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = userEmail,
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
        [ValidateAntiForgeryToken]
        public IActionResult ProcessBillPayment(decimal amount, string provider, string phoneNumber)
        {
            string userEmail = GetLoggedInUser();

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);

            if (wallet == null || wallet.Balance < amount)
            {
                TempData["Error"] = "Insufficient funds!";
                return RedirectToAction("PayBills");
            }

            wallet.Balance -= amount;

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "BILL-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = userEmail,
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
        [ValidateAntiForgeryToken]
        public IActionResult CreateThriftPlan(string title, decimal targetAmount, string frequency, int durationMonths)
        {
            string userEmail = GetLoggedInUser();

            _context.ThriftPlans.Add(new ThriftPlan
            {
                UserEmail = userEmail,
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
        [ValidateAntiForgeryToken]
        public IActionResult SaveToThrift(int planId, decimal amount)
        {
            string userEmail = GetLoggedInUser();

            var wallet = _context.Wallets
                .FirstOrDefault(w => w.UserEmail == userEmail);

            var plan = _context.ThriftPlans
                .FirstOrDefault(p => p.Id == planId && p.UserEmail == userEmail);

            if (wallet == null || wallet.Balance < amount)
            {
                TempData["Error"] = "Insufficient funds!";
                return RedirectToAction("Thrift");
            }

            if (plan == null)
            {
                TempData["Error"] = "Savings plan not found.";
                return RedirectToAction("Thrift");
            }

            wallet.Balance -= amount;
            plan.CurrentSavings += amount;

            _context.Transactions.Add(new TransactionRecord
            {
                Reference = "SAV-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                UserEmail = userEmail,
                Amount = amount * -1,
                Description = $"Savings: {plan.Title}",
                Date = DateTime.UtcNow,
                Status = "Success"
            });
            _context.SaveChanges();

            TempData["Message"] = "Savings Updated!";
            return RedirectToAction("Thrift");
        }

        // ─── SYSTEM ──────────────────────────────────────────────────────────

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