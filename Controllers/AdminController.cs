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
        // 📊 1. ANALYTICS ZONE
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
                TotalSystemBalance = totalWalletBalance + totalThriftBalance,
                PendingSupportCount = await _context.SupportTickets.AsNoTracking().CountAsync(t => t.Status == "Open"),
                FlaggedTransactionCount = await _context.Transactions.AsNoTracking().CountAsync(t => t.Status == "Flagged"),
                AllUsers = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
                PendingTransactions = await _context.Transactions.AsNoTracking().Where(t => t.Status == "Pending").OrderByDescending(t => t.Date).ToListAsync(),
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking().Include(g => g.MemberPlans).ToListAsync()
            };

            return View(viewModel);
        }

        // ============================================================
        // 👥 2. IDENTITY ZONE (KYC & Toggle Status)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var users = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        // ❄️ USER FREEZE/UNFREEZE LOGIC (Route Fix applied)
        [HttpPost]
        [Route("Admin/ToggleUserStatus/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return Unauthorized();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // World-Class Toggle Logic
            user.Status = (user.Status == "Active") ? "Frozen" : "Active";

            await _context.SaveChangesAsync();
            TempData["Success"] = $"User {user.FullName} status updated to: {user.Status}";

            return RedirectToAction(nameof(UserManagement));
        }

        // ============================================================
        // 🛡️ 3. SECURITY & RISK ZONE
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
        // 💸 4. DISBURSEMENT ZONE
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> PayoutControl()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking().Where(t => t.Status == "Pending").OrderByDescending(t => t.Date).ToListAsync(),
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking().Include(g => g.MemberPlans).ToListAsync()
            };
            return View(viewModel);
        }

        // ============================================================
        // 🛠️ 5. OPERATIONS ZONE
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SupportCenter()
        {
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            var viewModel = new AdminDashboardViewModel
            {
                SupportTickets = await _context.SupportTickets.AsNoTracking().OrderByDescending(t => t.CreatedAt).ToListAsync()
            };
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