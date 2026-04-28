using Dabbasheth.Data;
using Dabbasheth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dabbasheth.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context) => _context = context;

        private string? Email() => TempData.Peek("UserEmail")?.ToString();
        private const string AdminEmail = "adejazzmind@gmail.com";

        // ── DASHBOARD ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var unreadCount = await _context.ChatMessages.AsNoTracking()
                .CountAsync(m => m.ReceiverEmail.ToLower() == email.ToLower() && !m.IsRead);

            var vm = new UserDashboardViewModel
            {
                Wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower()),
                ThriftPlans = await _context.ThriftPlans.Where(p => p.UserEmail.ToLower() == email.ToLower()).ToListAsync(),
                RecentTransactions = await _context.Transactions.Where(t => t.UserEmail.ToLower() == email.ToLower()).OrderByDescending(t => t.Date).Take(5).ToListAsync(),
                ThriftGroups = await _context.ThriftGroups.Include(g => g.MemberPlans).Where(g => g.MemberPlans.Any(m => m.UserEmail.ToLower() == email.ToLower())).ToListAsync()
            };
            ViewBag.UnreadMessages = unreadCount;
            return View(vm);
        }

        // ── CUSTOMER CHAT WITH ADMIN ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Chat()
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");

            var messages = await _context.ChatMessages.AsNoTracking()
                .Where(m => (m.SenderEmail.ToLower() == email.ToLower() && m.ReceiverEmail == AdminEmail) ||
                             (m.SenderEmail == AdminEmail && m.ReceiverEmail.ToLower() == email.ToLower()))
                .OrderBy(m => m.SentAt).ToListAsync();

            // Mark admin messages as read
            var unread = await _context.ChatMessages
                .Where(m => m.ReceiverEmail.ToLower() == email.ToLower() && m.SenderEmail == AdminEmail && !m.IsRead).ToListAsync();
            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            ViewBag.Messages = messages;
            ViewBag.UserEmail = email;
            ViewBag.AdminEmail = AdminEmail;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string message)
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            if (!string.IsNullOrWhiteSpace(message))
            {
                _context.ChatMessages.Add(new ChatMessage { SenderEmail = email, ReceiverEmail = AdminEmail, Message = message.Trim(), IsAdminMessage = false, SentAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Chat));
        }

        // ── WITHDRAWAL ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Withdraw()
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            ViewBag.Balance = wallet?.Balance ?? 0m;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitWithdrawal(decimal amount, string bankName, string accountNumber)
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            if (wallet == null || amount <= 0 || wallet.Balance < amount) { TempData["Error"] = "Insufficient funds or invalid amount."; return RedirectToAction("Withdraw"); }
            _context.Transactions.Add(new Transaction { UserEmail = email, Amount = amount, Type = "Debit", Description = $"Withdrawal: {bankName} ({accountNumber})", Date = DateTime.UtcNow, Status = "Pending" });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Withdrawal request submitted — awaiting admin approval.";
            return RedirectToAction("Index");
        }

        // ── THRIFT ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Thrift()
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            var plans = await _context.ThriftPlans.Where(p => p.UserEmail.ToLower() == email.ToLower()).ToListAsync();
            ViewBag.ThriftPlans = plans;
            ViewBag.TotalSavings = plans.Sum(p => (decimal?)p.CurrentSavings) ?? 0m;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftPlan(string title, decimal targetAmount, string frequency)
        {
            var email = Email();
            if (string.IsNullOrEmpty(email) || targetAmount <= 0) { TempData["Error"] = "Invalid details."; return RedirectToAction("Thrift"); }
            _context.ThriftPlans.Add(new ThriftPlan { Title = title.Trim(), TargetAmount = targetAmount, Frequency = frequency ?? "Daily", UserEmail = email, Status = "Active", StartDate = DateTime.UtcNow, MaturityDate = DateTime.UtcNow.AddMonths(6) });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Savings goal created!";
            return RedirectToAction("Thrift");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DepositToThrift(int planId, decimal amount)
        {
            var email = Email();
            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login", "Account");
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            var plan = await _context.ThriftPlans.FindAsync(planId);
            if (wallet == null || plan == null || amount <= 0 || wallet.Balance < amount) { TempData["Error"] = "Insufficient balance."; return RedirectToAction("Thrift"); }
            wallet.Balance -= amount; plan.CurrentSavings += amount;
            _context.Transactions.Add(new Transaction { UserEmail = email, Amount = amount, Type = "Debit", Description = $"Savings: {plan.Title}", Date = DateTime.UtcNow, Status = "Success" });
            if (plan.CurrentSavings >= plan.TargetAmount) plan.Status = "Completed";
            await _context.SaveChangesAsync();
            TempData["Message"] = $"₦{amount:N2} added to savings!";
            return RedirectToAction("Thrift");
        }

        // ── UTILITIES ─────────────────────────────────────────────────
        [HttpGet] public IActionResult Airtime() { if (Email() == null) return RedirectToAction("Login", "Account"); return View(); }
        [HttpGet] public IActionResult Data() { if (Email() == null) return RedirectToAction("Login", "Account"); return View(); }
        [HttpGet] public IActionResult PayBills() { if (Email() == null) return RedirectToAction("Login", "Account"); return View(); }
        [HttpGet] public IActionResult Rewards() { ViewBag.Cashback = 36.00m; return View(); }
        public IActionResult Error() => View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}