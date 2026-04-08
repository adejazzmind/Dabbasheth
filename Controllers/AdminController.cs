using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using System.Linq;
using System;
using System.Collections.Generic;

namespace Dabbasheth.Controllers
{
    public class AdminController : Controller
    {
        // --- ADMIN DASHBOARD ---
        [HttpGet]
        public IActionResult Index()
        {
            // Security: Only Samson or Tolu (Roles marked as Admin) can enter
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin")
                return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = MockDatabase.Users.Count(u => u.Role == "Customer"),
                TotalSystemBalance = MockDatabase.Wallets.Sum(w => w.Balance),
                AllUsers = MockDatabase.Users.ToList(),
                AllWallets = MockDatabase.Wallets.ToList(),
                AllThriftPlans = MockDatabase.ThriftPlans.ToList()
            };

            return View(viewModel);
        }

        // --- THE "GOD MODE" CREDIT ACTION ---
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreditAccount(string targetEmail, decimal amount, string accountType, int? planId)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            if (amount <= 0) return RedirectToAction("Index");

            // LOGIC: Credit Wallet
            if (accountType == "Wallet")
            {
                var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == targetEmail);
                if (wallet != null)
                {
                    wallet.Balance += amount;
                    AddAdminLog(targetEmail, amount, $"Credit: Wallet");
                }
            }
            // LOGIC: Credit Thrift/Savings (Weekly, Monthly, etc)
            else if (accountType == "Thrift" && planId.HasValue)
            {
                var plan = MockDatabase.ThriftPlans.FirstOrDefault(p => p.Id == planId.Value);
                if (plan != null)
                {
                    plan.CurrentSavings += amount;
                    AddAdminLog(targetEmail, amount, $"Credit Thrift: {plan.Title}");
                }
            }

            TempData["Message"] = "Account Credited Successfully!";
            return RedirectToAction("Index");
        }

        private void AddAdminLog(string email, decimal amount, string note)
        {
            var adminName = TempData.Peek("UserName")?.ToString() ?? "System";

            MockDatabase.Transactions.Add(new TransactionRecord
            {
                Reference = "ADM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                UserEmail = email,
                Amount = amount,
                Description = $"{note} (By {adminName})",
                Date = DateTime.Now,
                Status = "Approved"
            });
        }
    }
}