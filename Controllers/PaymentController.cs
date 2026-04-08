using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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

        // --- STEP 1: BYPASSED INITIALIZE PAYMENT ---
        [HttpPost]
        public async Task<IActionResult> InitializePayment(string email, int amount)
        {
            try
            {
                // 🛑 BYPASS MODE: Money is added directly to Neon without calling Paystack
                string mockReference = "BYPASS-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                // Call the internal wallet logic that talks to Neon
                bool isUpdated = await UpdateUserWallet(email, (decimal)amount);

                if (isUpdated)
                {
                    // Create a real transaction record in Neon
                    var txn = new TransactionRecord
                    {
                        Reference = mockReference,
                        UserEmail = email,
                        Amount = (decimal)amount,
                        Description = "Wallet Top-up (Bypass Mode)",
                        Date = DateTime.UtcNow,
                        Status = "Success"
                    };

                    _context.Transactions.Add(txn);
                    await _context.SaveChangesAsync();

                    ViewBag.Message = $"Success: ₦{amount} has been added to your REAL Neon wallet.";
                    return View("Success");
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return Content($"Bypass Error: {ex.Message}");
            }
        }

        // --- STEP 2: VERIFY (Route Safety) ---
        [HttpGet]
        public IActionResult Verify(string reference)
        {
            return RedirectToAction("Index", "Home");
        }

        // --- STEP 3: WALLET LOGIC (Talking to REAL Database) ---
        private async Task<bool> UpdateUserWallet(string email, decimal amount)
        {
            // Search the REAL Neon database for this user's wallet
            var wallet = _context.Wallets.FirstOrDefault(w => w.UserEmail == email);

            if (wallet != null)
            {
                wallet.Balance += amount;
            }
            else
            {
                // If wallet doesn't exist, create it in Neon
                _context.Wallets.Add(new Wallet
                {
                    UserEmail = email,
                    Balance = amount,
                    CreatedAt = DateTime.UtcNow,
                    Currency = "NGN"
                });
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}