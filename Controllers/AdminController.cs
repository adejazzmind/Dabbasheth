using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1. DASHBOARD - CONSOLIDATED CEO COMMAND CENTER
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            // 🚀 FINANCIAL AGGREGATION ENGINE
            // Calculates total liquidity across all user wallets and thrift plans
            var totalWalletBalance = await _context.Wallets.SumAsync(w => (decimal?)w.Balance) ?? 0m;
            var totalThriftBalance = await _context.ThriftPlans.SumAsync(p => (decimal?)p.CurrentSavings) ?? 0m;
            var grandTotalLiquidity = totalWalletBalance + totalThriftBalance;

            // 🛡️ ACTION REQUIRED: Fetch all transactions awaiting Admin approval
            var pendingRequests = await _context.Transactions
                .Where(t => t.Status == "Pending")
                .OrderByDescending(t => t.Date)
                .ToListAsync();

            // 📊 ASSEMBLE COMPLETE VIEW MODEL
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(u => u.Role == "Customer"),
                TotalSystemBalance = grandTotalLiquidity,
                AllUsers = await _context.Users.ToListAsync(),
                AllWallets = await _context.Wallets.ToListAsync(),

                // ✅ FETCH AJO GROUPS (Dynamic Data for the Table)
                AllThriftGroups = await _context.ThriftGroups
                    .OrderByDescending(g => g.StartDate)
                    .ToListAsync(),

                // MONITORING: All individual savings goals
                AllThriftPlans = await _context.ThriftPlans
                    .OrderByDescending(p => p.StartDate)
                    .ToListAsync(),

                PendingTransactions = pendingRequests
            };

            return View(viewModel);
        }

        // ============================================================
        // 2. TRANSACTION GOVERNANCE (Withdrawals)
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTransaction(string reference)
        {
            try
            {
                var tx = await _context.Transactions.FirstOrDefaultAsync(t => t.Reference == reference);
                if (tx == null || tx.Status != "Pending") return RedirectToAction("Index");

                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail == tx.UserEmail);
                if (wallet == null || wallet.Balance < tx.Amount)
                {
                    TempData["Error"] = "❌ Approval Failed: User has insufficient funds.";
                    return RedirectToAction("Index");
                }

                // Execute Financial Transfer
                wallet.Balance -= tx.Amount;
                tx.Status = "Success";
                tx.Date = DateTime.UtcNow;

                _context.Wallets.Update(wallet);
                _context.Transactions.Update(tx);

                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Withdrawal approved and funds released.";
            }
            catch (Exception ex) { TempData["Error"] = "System Error: " + ex.Message; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineTransaction(string reference)
        {
            var tx = await _context.Transactions.FirstOrDefaultAsync(t => t.Reference == reference);
            if (tx != null && tx.Status == "Pending")
            {
                tx.Status = "Declined";
                await _context.SaveChangesAsync();
                TempData["Message"] = "❌ Transaction Request Declined.";
            }
            return RedirectToAction("Index");
        }

        // ============================================================
        // 3. REGULATION - WALLET & INDIVIDUAL THRIFT OVERRIDES
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditAccount(string targetEmail, decimal amount, string accountType)
        {
            if (string.IsNullOrEmpty(targetEmail) || amount <= 0) return RedirectToAction("Index");

            try
            {
                string cleanEmail = targetEmail.Trim().ToLower();
                if (accountType == "Wallet")
                {
                    var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == cleanEmail);
                    if (wallet != null) wallet.Balance += amount;
                    else throw new Exception("Wallet not found.");
                }
                else if (accountType == "Thrift")
                {
                    var plan = await _context.ThriftPlans
                        .Where(p => p.UserEmail.ToLower() == cleanEmail && p.Status == "Active")
                        .OrderByDescending(p => p.StartDate)
                        .FirstOrDefaultAsync();

                    if (plan != null) plan.CurrentSavings += amount;
                    else throw new Exception("No active Thrift Plan found.");
                }

                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Credited {accountType} with ₦{amount:N2}";
            }
            catch (Exception ex) { TempData["Error"] = "Credit Failed: " + ex.Message; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DebitAccount(string targetEmail, decimal amount, string reason)
        {
            try
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == targetEmail.ToLower());
                if (wallet != null)
                {
                    wallet.Balance -= amount;
                    _context.Transactions.Add(new TransactionRecord
                    {
                        Reference = "DEB-" + Guid.NewGuid().ToString()[..8].ToUpper(),
                        UserEmail = targetEmail.ToLower(),
                        Amount = amount,
                        Description = "Admin Debit: " + (reason ?? "Regulatory adjustment"),
                        Date = DateTime.UtcNow,
                        Status = "Success"
                    });
                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"⚠️ Debited ₦{amount:N2} from {targetEmail}";
                }
            }
            catch (Exception ex) { TempData["Error"] = "Debit Failed: " + ex.Message; }
            return RedirectToAction("Index");
        }

        // ============================================================
        // 4. GROUP THRIFT (AJO) - COLLECTION & INITIALIZATION
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftGroup(ThriftGroup group)
        {
            try
            {
                group.Status = "Running";
                group.StartDate = DateTime.UtcNow;

                _context.ThriftGroups.Add(group);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Group '{group.GroupName}' initialized successfully.";
            }
            catch (Exception ex) { TempData["Error"] = "Setup Failed: " + ex.Message; }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExecuteGroupCollection(int groupId)
        {
            var group = await _context.ThriftGroups
                .Include(g => g.MemberPlans)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return RedirectToAction("Index");

            int successCount = 0;
            int failCount = 0;

            foreach (var plan in group.MemberPlans)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail == plan.UserEmail);
                if (wallet != null && wallet.Balance >= group.MonthlyContribution)
                {
                    wallet.Balance -= group.MonthlyContribution;
                    plan.CurrentSavings += group.MonthlyContribution;
                    _context.Transactions.Add(new TransactionRecord
                    {
                        Reference = $"AJO-{group.Id}-{Guid.NewGuid().ToString()[..4].ToUpper()}",
                        UserEmail = plan.UserEmail,
                        Amount = group.MonthlyContribution,
                        Description = $"Ajo Collection: {group.GroupName}",
                        Status = "Success",
                        Date = DateTime.UtcNow
                    });
                    successCount++;
                }
                else { failCount++; }
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"✅ Ajo Complete: {successCount} Paid, {failCount} Insufficient Funds.";
            return RedirectToAction("Index");
        }

        // ============================================================
        // 5. AJO PAYOUT LOGIC (The "Monthly Pack")
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMonthlyPayout(int groupId, int currentMonthOrder)
        {
            var group = await _context.ThriftGroups
                .Include(g => g.MemberPlans)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return RedirectToAction("Index");

            // Calculate Bulk Payout based on category amount and participant count
            decimal bulkAmount = group.CategoryAmount * group.MemberPlans.Count;

            // Identify the recipient for the specific month slot
            var recipientPlan = group.MemberPlans.FirstOrDefault(p => p.PayoutOrder == currentMonthOrder);

            if (recipientPlan != null)
            {
                var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail == recipientPlan.UserEmail);
                if (wallet != null)
                {
                    wallet.Balance += bulkAmount;
                    recipientPlan.HasCollected = true;

                    _context.Transactions.Add(new TransactionRecord
                    {
                        Reference = $"PAY-{group.Id}-{DateTime.Now:MMM}".ToUpper(),
                        UserEmail = recipientPlan.UserEmail,
                        Amount = bulkAmount,
                        Description = $"Ajo Payout Received: {group.GroupName} (Month {currentMonthOrder})",
                        Status = "Success",
                        Date = DateTime.UtcNow
                    });

                    await _context.SaveChangesAsync();
                    TempData["Message"] = $"💰 ₦{bulkAmount:N0} successfully packed by {recipientPlan.UserEmail}!";
                }
            }
            return RedirectToAction("Index");
        }

        // ============================================================
        // 6. INDIVIDUAL THRIFT MANAGEMENT
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveThrift(int planId)
        {
            var plan = await _context.ThriftPlans.FindAsync(planId);
            if (plan != null)
            {
                plan.Status = "Active";
                await _context.SaveChangesAsync();
                TempData["Message"] = "✅ Thrift goal approved.";
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookThrift(string targetEmail, string title, decimal targetAmount, string frequency)
        {
            try
            {
                var newPlan = new ThriftPlan
                {
                    Title = title,
                    TargetAmount = targetAmount,
                    CurrentSavings = 0,
                    Frequency = frequency,
                    UserEmail = targetEmail.Trim().ToLower(),
                    Status = "Active",
                    StartDate = DateTime.UtcNow
                };
                _context.ThriftPlans.Add(newPlan);
                await _context.SaveChangesAsync();
                TempData["Message"] = $"✅ Booked '{title}' for {targetEmail}";
            }
            catch (Exception ex) { TempData["Error"] = "Booking Failed: " + ex.Message; }
            return RedirectToAction("Index");
        }
    }
}