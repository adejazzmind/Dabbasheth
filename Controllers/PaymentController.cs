using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        public PaymentController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public IActionResult TopUp()
        {
            if (TempData.Peek("UserEmail") == null) return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitializePayment(string email, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(email) || amount <= 0)
            {
                TempData["Error"] = "Invalid transaction details.";
                return RedirectToAction("Index", "Home");
            }
            try
            {
                var clean = email.Trim().ToLower();
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == clean);
                if (wallet != null)
                    wallet.Balance += amount;
                else
                    _context.Wallets.Add(new Wallet
                    {
                        UserEmail = clean,
                        Balance = amount,
                        Currency = "NGN",
                        WalletNumber = "DAB-" + new Random().Next(10000000, 99999999),
                        CreatedAt = DateTime.UtcNow
                    });

                _context.Transactions.Add(new Transaction
                {
                    UserEmail = clean,
                    Amount = amount,
                    Type = "Credit",
                    Description = "Wallet Top-up",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });
                await _context.SaveChangesAsync();
                ViewBag.Message = $"₦{amount:N2} successfully added to your wallet.";
                return View("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Payment error: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet] public IActionResult Verify(string reference) => RedirectToAction("Index", "Home");
        [HttpGet] public IActionResult Success() => View();
    }
}
