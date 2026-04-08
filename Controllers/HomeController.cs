using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System;

namespace Dabbasheth.Controllers
{
    public class HomeController : Controller
    {
        /* --- SECTION 1: UTILITIES --- */

        private string GetLoggedInUser()
        {
            var email = TempData.Peek("UserEmail") as string;
            // Falls back to MD's official email for testing
            if (string.IsNullOrEmpty(email)) return "adejazzmind@gmail.com";
            return email;
        }

        /* --- SECTION 2: VIEW ACTIONS (GET) --- */

        public IActionResult Index()
        {
            string userEmail = GetLoggedInUser();
            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);

            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            ViewBag.UserEmail = userEmail;

            var transactions = MockDatabase.Transactions
                .Where(t => t.UserEmail == userEmail)
                .OrderByDescending(t => t.Date)
                .ToList();

            return View(transactions);
        }

        public IActionResult Thrift()
        {
            string userEmail = GetLoggedInUser();
            var plans = MockDatabase.ThriftPlans
                .Where(p => p.UserEmail == userEmail && p.Status == "Active")
                .ToList();

            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;

            return View(plans);
        }

        public IActionResult Withdraw()
        {
            string userEmail = GetLoggedInUser();
            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        public IActionResult PayBills()
        {
            string userEmail = GetLoggedInUser();
            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
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
            string userEmail = GetLoggedInUser();
            ViewBag.Cashback = 36.50m;
            return View();
        }

        public IActionResult Profile()
        {
            string userEmail = GetLoggedInUser();
            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == userEmail);
            ViewBag.UserEmail = userEmail;
            ViewBag.Balance = wallet?.Balance ?? 0.00m;
            return View();
        }

        /* --- SECTION 3: TRANSACTION PROCESSING (POST) --- */

        // THE MISSING FIX: Processes Loan Applications
        [HttpPost]
        public IActionResult ProcessLoan(decimal loanAmount)
        {
            string userEmail = GetLoggedInUser();

            MockDatabase.Transactions.Insert(0, new TransactionRecord
            {
                Reference = "LOAN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = userEmail,
                Amount = loanAmount,
                Description = $"IES Loan Application: ₦{loanAmount:N2}",
                Date = DateTime.Now,
                Status = "Pending Approval"
            });

            TempData["Message"] = "Loan application submitted! Admin (MD/CEO) will review shortly.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            string userEmail = GetLoggedInUser();

            MockDatabase.Transactions.Insert(0, new TransactionRecord
            {
                Reference = "WTH-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = userEmail,
                Amount = amount * -1,
                Description = $"Withdrawal to {bankName} ({accountNumber})",
                Date = DateTime.Now,
                Status = "Pending Approval"
            });

            TempData["Message"] = "Withdrawal request sent!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ProcessBillPayment(decimal amount, string provider, string phoneNumber)
        {
            string userEmail = GetLoggedInUser();

            MockDatabase.Transactions.Insert(0, new TransactionRecord
            {
                Reference = "BILL-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = userEmail,
                Amount = amount * -1,
                Description = $"{provider} Airtime: {phoneNumber}",
                Date = DateTime.Now,
                Status = "Success"
            });

            TempData["Message"] = "Payment Successful!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CreateThriftPlan(string title, decimal targetAmount, string frequency, int durationMonths)
        {
            string userEmail = GetLoggedInUser();
            MockDatabase.ThriftPlans.Add(new ThriftPlan
            {
                Id = MockDatabase.ThriftPlans.Count + 1,
                UserEmail = userEmail,
                Title = title,
                TargetAmount = targetAmount,
                CurrentSavings = 0,
                Frequency = frequency,
                StartDate = DateTime.Now,
                MaturityDate = DateTime.Now.AddMonths(durationMonths),
                Status = "Active"
            });

            TempData["Message"] = "Goal Created Successfully!";
            return RedirectToAction("Thrift");
        }

        [HttpPost]
        public IActionResult SaveToThrift(int planId, decimal amount)
        {
            string userEmail = GetLoggedInUser();
            var plan = MockDatabase.ThriftPlans.FirstOrDefault(p => p.Id == planId);

            if (plan != null)
            {
                plan.CurrentSavings += amount;
                MockDatabase.Transactions.Insert(0, new TransactionRecord
                {
                    Reference = "SAV-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    UserEmail = userEmail,
                    Amount = amount * -1,
                    Description = $"Savings: {plan.Title}",
                    Date = DateTime.Now,
                    Status = "Success"
                });
            }

            TempData["Message"] = "Savings Updated!";
            return RedirectToAction("Thrift");
        }

        /* --- SECTION 4: SYSTEM --- */

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}