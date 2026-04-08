using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;
            // PayStackApi initialization removed to prevent crashes if keys are missing
        }

        // --- STEP 1: BYPASSED INITIALIZE PAYMENT ---
        [HttpPost]
        public async Task<IActionResult> InitializePayment(string email, int amount)
        {
            try
            {
                // 🛑 BYPASS: Skipping the real Paystack API call entirely.
                // Instead, we act as if the user already paid and go straight to verification logic.

                string mockReference = "BYPASS-" + Guid.NewGuid().ToString().Substring(0, 8);

                // We directly call our internal wallet logic
                bool isUpdated = await UpdateUserWallet(email, (decimal)amount);

                if (isUpdated)
                {
                    // Log to mock history so the dashboard shows the "transaction"
                    MockDatabase.Transactions.Insert(0, new TransactionRecord
                    {
                        Reference = mockReference,
                        Amount = amount,
                        Date = DateTime.Now,
                        Status = "Success (Bypass Mode)"
                    });

                    ViewBag.Message = $"Development Mode: ₦{amount} has been added to your wallet without Paystack.";
                    return View("Success");
                }

                return RedirectToAction("Index", "Dashboard");
            }
            catch (Exception ex)
            {
                // This prevents the red "Something went wrong" screen from crashing the app
                return Content($"Bypass Error: {ex.Message}");
            }
        }

        // --- STEP 2: VERIFY (Kept for routing safety, but Initialize handles it now) ---
        [HttpGet]
        public IActionResult Verify(string reference)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        // --- STEP 3: WALLET LOGIC (Required to see balance changes) ---
        private async Task<bool> UpdateUserWallet(string email, decimal amount)
        {
            var wallet = MockDatabase.Wallets.FirstOrDefault(w => w.UserEmail == email);

            if (wallet != null)
            {
                wallet.Balance += amount;
            }
            else
            {
                MockDatabase.Wallets.Add(new Wallet
                {
                    Id = MockDatabase.Wallets.Count + 1,
                    UserEmail = email,
                    Balance = amount
                });
            }

            return true;
        }
    }
}