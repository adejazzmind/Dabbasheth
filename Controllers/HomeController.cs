using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System;

namespace Dabbasheth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GetLoggedInUser()
        {
            var email = TempData.Peek("UserEmail") as string;
            return string.IsNullOrEmpty(email) ? "adejazzmind@gmail.com" : email;
        }

        public IActionResult Index()
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);

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

            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View(plans);
        }

        public IActionResult Withdraw()
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        [HttpPost]
        public IActionResult ProcessLoan(decimal loanAmount)
        {
            string userEmail = GetLoggedInUser();
            var txn = new TransactionRecord
            {
                Reference = "LOAN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = userEmail,
                Amount = loanAmount,
                Description = $"IES Loan Application: ₦{loanAmount:N2}",
                Date = DateTime.UtcNow,
                Status = "Pending Approval"
            };

            _context.Transactions.Add(txn);
            _context.SaveChanges();

            TempData["Message"] = "Loan application submitted!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            string userEmail = GetLoggedInUser();
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);

            if (wallet == null || wallet.Balance < amount)
            {
                TempData["Error"] = "Insufficient funds!";
                return RedirectToAction("Withdraw");
            }

            var txn = new TransactionRecord
            {
                Reference = "WTH-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = userEmail,
                Amount = amount * -1,
                Description = $"Withdrawal to {bankName} ({accountNumber})",
                Date = DateTime.UtcNow,
                Status = "Pending Approval"
            };

            _context.Transactions.Add(txn);
            _context.SaveChanges();

            TempData["Message"] = "Withdrawal request sent!";
            return RedirectToAction("Index");
        }

        public IActionResult PayBills() => View();
        public IActionResult Loan() => View();
        public IActionResult Rewards() => View();
        public IActionResult Profile() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}