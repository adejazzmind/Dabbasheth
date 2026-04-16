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
        // 📊 1. ANALYTICS ZONE (Executive Dashboard Overview)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // 🛡️ Gatekeeper: Secure Session Verification
            var role = TempData.Peek("UserRole")?.ToString();
            if (role != "Admin") return RedirectToAction("Login", "Account");

            // 💰 Safe Global Liquidity Calculations (Case-Insensitive)
            // We use (decimal?) to handle null sums on empty tables safely
            var totalWalletBalance = await _context.Wallets.AsNoTracking().SumAsync(w => (decimal?)w.Balance) ?? 0m;
            var totalThriftBalance = await _context.ThriftPlans.AsNoTracking().SumAsync(p => (decimal?)p.CurrentSavings) ?? 0m;

            // ✅ ROBUST DATA RETRIEVAL: 
            // We avoid sorting by 'Id' to prevent PostgreSQL case-sensitivity crashes.
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(u => u.Role == "Customer"),
                TotalSystemBalance = totalWalletBalance + totalThriftBalance,
                PendingSupportCount = await _context.SupportTickets.AsNoTracking().CountAsync(t => t.Status == "Open"),
                FlaggedTransactionCount = await _context.Transactions.AsNoTracking().CountAsync(t => t.Status == "Flagged"),

                AllUsers = await _context.Users.AsNoTracking()
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5).ToListAsync(),

                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending")
                    .OrderByDescending(t => t.Date)
                    .ToListAsync(),

                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                    .Include(g => g.MemberPlans)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // ============================================================
        // 👥 2. USER MANAGEMENT ZONE (Identity & KYC)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            var users = await _context.Users.AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View(users);
        }

        // ============================================================
        // 💸 3. DISBURSEMENT ZONE (Payout Control)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> PayoutControl()
        {
            var viewModel = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending")
                    .OrderByDescending(t => t.Date)
                    .ToListAsync(),

                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                    .Include(g => g.MemberPlans)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // ============================================================
        // 🛠️ 4. HUB OPERATIONS (Support & System Settings)
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> SupportCenter()
        {
            var viewModel = new AdminDashboardViewModel
            {
                SupportTickets = await _context.SupportTickets.AsNoTracking()
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync()
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            // Fetch system settings list for the configuration view
            var settings = await _context.SystemSettings.AsNoTracking().ToListAsync();
            return View(settings);
        }
    }
}