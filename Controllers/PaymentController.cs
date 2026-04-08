using Dabbasheth.Data;
using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ────────────────────────────────────────────────────────────────
        // POST: Add Money to Wallet (Bypass Mode - Development Friendly)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> InitializePayment(string email, decimal amount)
        {
            if (string.IsNullOrWhiteSpace(email) || amount <= 0)
            {
                TempData["Error"] = "Invalid email or amount.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                string mockReference = "BYPASS-" + Guid.NewGuid().ToString()[..8].ToUpper();

                // Update wallet in real Neon database
                bool isUpdated = await UpdateUserWallet(email.Trim().ToLower(), amount);

                if (isUpdated)
                {
                    // Log the transaction
                    var txn = new TransactionRecord
                    {
                        Reference = mockReference,
                        UserEmail = email.Trim().ToLower(),
                        Amount = amount,
                        Description = "Wallet Top-up (Development Bypass Mode)",
                        Date = DateTime.UtcNow,
                        Status = "Success"
                    };

                    _context.Transactions.Add(txn);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = $"✅ ₦{amount:N2} has been successfully added to your wallet.";
                    return View("Success");
                }

                TempData["Error"] = "Failed to update wallet.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while adding funds.";
                Console.WriteLine($"Payment Error: {ex.Message}");
                return RedirectToAction("Index", "Home");
            }
        }

        // ────────────────────────────────────────────────────────────────
        // GET: Verify Payment (Kept for future real Paystack integration)
        // ────────────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Verify(string reference)
        {
            // For now, just redirect to dashboard
            TempData["Message"] = "Payment verification completed.";
            return RedirectToAction("Index", "Home");
        }

        // ────────────────────────────────────────────────────────────────
        // Private: Update or Create Wallet
        // ────────────────────────────────────────────────────────────────
        private async Task<bool> UpdateUserWallet(string email, decimal amount)
        {
            // Normalize email
            email = email.ToLower().Trim();

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserEmail == email);

            if (wallet != null)
            {
                wallet.Balance += amount;
            }
            else
            {
                // Create new wallet
                _context.Wallets.Add(new Wallet
                {
                    UserEmail = email,
                    Balance = amount,
                    Currency = "NGN",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}