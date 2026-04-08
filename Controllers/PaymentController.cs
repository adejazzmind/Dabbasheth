using Dabbasheth.Data;
using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> InitializePayment(string email, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(email) || amount <= 0)
            {
                TempData["Error"] = "Invalid amount or email.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var cleanEmail = email.Trim().ToLower();

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserEmail == cleanEmail);

                if (wallet != null)
                    wallet.Balance += amount;
                else
                {
                    _context.Wallets.Add(new Wallet
                    {
                        UserEmail = cleanEmail,
                        Balance = amount,
                        Currency = "NGN",
                        CreatedAt = DateTime.UtcNow
                    });
                }

                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = "BYPASS-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                    UserEmail = cleanEmail,
                    Amount = amount,
                    Description = "Direct Wallet Top-up",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                await _context.SaveChangesAsync();

                ViewBag.Message = $"✅ ₦{amount:N2} added to your wallet successfully!";
                return View("Success");
            }
            catch
            {
                TempData["Error"] = "Failed to add money.";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Verify(string reference) => RedirectToAction("Index", "Home");
    }
}