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

        // --- STEP 1: BYPASSED INITIALIZE PAYMENT ---
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

                // 1. Update or Create Wallet
                if (wallet != null)
                {
                    wallet.Balance += amount;
                }
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

                // 2. Log Transaction
                string reference = "DB-BYPASS-" + Guid.NewGuid().ToString()[..8].ToUpper();
                _context.Transactions.Add(new TransactionRecord
                {
                    Reference = reference,
                    UserEmail = cleanEmail,
                    Amount = amount,
                    Description = "Wallet Top-up (Development Bypass)",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                await _context.SaveChangesAsync();

                ViewBag.Message = $"₦{amount:N2} has been successfully added to your wallet via Secure Bypass.";
                return View("Success");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Internal system error during bypass: " + ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }

        // Keep for routing compatibility
        [HttpGet]
        public IActionResult Verify(string reference) => RedirectToAction("Index", "Home");
    }
}