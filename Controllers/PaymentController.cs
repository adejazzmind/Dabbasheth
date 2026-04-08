using Microsoft.AspNetCore.Mvc;
using PayStack.Net;
using Dabbasheth.Models;
using Dabbasheth.Data;
using System.Diagnostics;

namespace Dabbasheth.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly PayStackApi _paystack;
        private readonly string _token;

        public PaymentController(IConfiguration configuration)
        {
            _configuration = configuration;

            // Pulls the secret key from appsettings.json
            _token = _configuration["Paystack:SecretKey"];
            _paystack = new PayStackApi(_token);
        }

        // --- STEP 1: INITIALIZE PAYMENT ---
        [HttpPost]
        public IActionResult InitializePayment(string email, int amount)
        {
            try
            {
                var request = new TransactionInitializeRequest
                {
                    AmountInKobo = amount * 100, // Naira to Kobo
                    Email = email,
                    Reference = Guid.NewGuid().ToString(),
                    CallbackUrl = "https://localhost:44300/Payment/Verify"
                };

                var response = _paystack.Transactions.Initialize(request);

                if (response != null && response.Status)
                {
                    return Redirect(response.Data.AuthorizationUrl);
                }

                return Content($"Paystack Error: {response?.Message ?? "Check Internet/Secret Key."}");
            }
            catch (Exception ex)
            {
                return Content($"System Error: {ex.Message}");
            }
        }

        // --- STEP 2: VERIFY PAYMENT & RECORD TRANSACTION ---
        [HttpGet]
        public async Task<IActionResult> Verify(string reference)
        {
            var response = _paystack.Transactions.Verify(reference);

            if (response.Status && response.Data.Status == "success")
            {
                var amountPaid = response.Data.Amount / 100;
                var userEmail = response.Data.Customer.Email;

                // 1. Update the User's Balance
                bool isUpdated = await UpdateUserWallet(userEmail, amountPaid);

                if (isUpdated)
                {
                    // 2. LOG TO TRANSACTION HISTORY
                    // Insert(0, ...) ensures the latest transaction is at the top
                    MockDatabase.Transactions.Insert(0, new TransactionRecord
                    {
                        Reference = reference,
                        Amount = amountPaid,
                        Date = DateTime.Now,
                        Status = "Success"
                    });

                    ViewBag.Message = $"Success! ₦{amountPaid} has been added to your wallet.";
                    return View("Success");
                }
            }

            ViewBag.Message = "We couldn't verify your payment. Please try again.";
            return View("Error");
        }

        // --- STEP 3: WALLET LOGIC ---
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