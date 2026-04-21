using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace Dabbasheth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
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
                var cleanEmail = email.Trim().ToLower();
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == cleanEmail);

                if (wallet != null)
                {
                    wallet.Balance += amount;
                }
                else
                {
                    _context.Wallets.Add(new Wallet { UserEmail = cleanEmail, Balance = amount, Currency = "NGN", CreatedAt = DateTime.UtcNow });
                }

                // ✅ FIXED: Changed TransactionRecord to Transaction (Lines 54-62)
                _context.Transactions.Add(new Transaction
                {
                    UserEmail = cleanEmail,
                    Amount = amount,
                    Type = "Credit",
                    Description = "Wallet Top-up (Development Bypass)",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                await _context.SaveChangesAsync();
                return View("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Internal system error: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Verify(string reference) => RedirectToAction("Index", "Home");
    }
}