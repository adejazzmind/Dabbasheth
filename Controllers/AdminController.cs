using Microsoft.AspNetCore.Mvc;
using Dabbasheth.Models;
using Dabbasheth.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

// ✅ ALIAS: Mapped to TransactionRecord to keep Model logic separate from property naming
using AdminTransaction = Dabbasheth.Models.TransactionRecord;

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
        // 📊 1. ANALYTICS ZONE (Executive Intelligence)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var totalWalletBalance = await _context.Wallets.AsNoTracking().SumAsync(w => (decimal?)w.Balance) ?? 0m;
            var totalThriftBalance = await _context.ThriftPlans.AsNoTracking().SumAsync(p => (decimal?)p.CurrentSavings) ?? 0m;

            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(u => u.Role == "Customer"),

                // ✅ POSTGRESQL FIX: Compares UTC Date only to prevent timestamp comparison exceptions
                NewUsersCount = await _context.Users.AsNoTracking()
                    .CountAsync(u => u.CreatedAt >= DateTime.UtcNow.Date),

                TotalSystemBalance = totalWalletBalance + totalThriftBalance,
                PendingSupportCount = await _context.SupportTickets.AsNoTracking().CountAsync(t => t.Status == "Open"),

                FlaggedTransactionCount = await _context.Transactions.AsNoTracking()
                    .CountAsync(t => t.Status == "Flagged"),

                AllUsers = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),

                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending")
                    .OrderByDescending(t => t.Date)
                    .ToListAsync(),

                AllThriftGroups = await _context.ThriftGroups.AsNoTracking().Include(g => g.MemberPlans).ToListAsync()
            };

            return View(viewModel);
        }

        // ============================================================
        // 👥 2. IDENTITY ZONE (User Management & KYC)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");
            var users = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpPost]
        [Route("Admin/ToggleUserStatus/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.Status = (user.Status == "Active") ? "Frozen" : "Active";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(UserManagement));
        }

        // ============================================================
        // 🛡️ 3. SECURITY & RISK ZONE (Fraud Prevention)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> RiskControl()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending" || t.Status == "Flagged")
                    .OrderByDescending(t => t.Date)
                    .ToListAsync(),
                AllUsers = await _context.Users.AsNoTracking().ToListAsync()
            };
            return View(viewModel);
        }

        // ============================================================
        // 💸 4. TREASURY ZONE (Disbursements & Payouts)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> PayoutControl()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending")
                    .OrderByDescending(t => t.Date)
                    .ToListAsync(),
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking().Include(g => g.MemberPlans).ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMonthlyPayout(int thriftGroupId, int memberPlanId)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            try
            {
                var plan = await _context.ThriftPlans
                    .Include(p => p.ThriftGroup)
                    .ThenInclude(g => g.MemberPlans)
                    .FirstOrDefaultAsync(p => p.Id == memberPlanId);

                if (plan == null || plan.HasCollected)
                {
                    TempData["Error"] = "Invalid payout request.";
                    return RedirectToAction(nameof(AjoManagement));
                }

                decimal totalPayout = plan.ThriftGroup.CategoryAmount * (plan.ThriftGroup.MemberPlans?.Count ?? 0);

                plan.HasCollected = true;
                plan.PayoutDate = DateTime.UtcNow; // Match DB Expectation

                var transaction = new AdminTransaction
                {
                    UserEmail = plan.UserEmail,
                    Amount = totalPayout,
                    Type = "Credit",
                    Description = $"Ajo Payout: {plan.ThriftGroup.GroupName} (Slot #{plan.PayoutOrder})",
                    Date = DateTime.UtcNow,
                    Status = "Completed"
                };

                // ✅ INSERT FIX: Handles insertion of Alias record automatically via standard tracking
                _context.Add(transaction);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Payout of ₦{totalPayout:N0} processed successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Payout failure: " + ex.Message;
            }

            return RedirectToAction(nameof(AjoManagement));
        }

        // ============================================================
        // ⚙️ 5. AJO ENGINE ZONE (Cycle Management)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> AjoManagement()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                    .Include(g => g.MemberPlans)
                    .OrderByDescending(g => g.Id)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftGroup(ThriftGroup group)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            ModelState.Remove("Status");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Frequency");
            ModelState.Remove("StartDate");

            if (ModelState.IsValid)
            {
                group.Status = "Active";
                group.Frequency = "Monthly";
                group.CreatedAt = DateTime.UtcNow;
                group.StartDate = DateTime.UtcNow;

                _context.ThriftGroups.Add(group);
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Cycle '{group.GroupName}' successfully initialized.";
            }
            return RedirectToAction(nameof(AjoManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMemberToGroup(int ThriftGroupId, string UserEmail, int PayoutOrder)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            try
            {
                var plan = new ThriftPlan
                {
                    ThriftGroupId = ThriftGroupId,
                    UserEmail = UserEmail,
                    PayoutOrder = PayoutOrder,
                    Title = "Ajo Membership Slot",
                    Status = "Active",
                    StartDate = DateTime.UtcNow,
                    Frequency = "Monthly"
                };
                _context.ThriftPlans.Add(plan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member successfully added to slot.";
            }
            catch (Exception ex) { TempData["Error"] = "Enrollment Error: " + ex.Message; }
            return RedirectToAction(nameof(AjoManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseGroup(int groupId)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();
            var group = await _context.ThriftGroups.FindAsync(groupId);
            if (group != null) { group.Status = "Closed"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(AjoManagement));
        }

        // ============================================================
        // 🛠️ 6. OPERATIONS ZONE (Platform Support & Config)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SupportCenter()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");
            var viewModel = new AdminDashboardViewModel { SupportTickets = await _context.SupportTickets.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync() };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");
            var settings = await _context.SystemSettings.AsNoTracking().ToListAsync();
            return View(settings);
        }
    }
}