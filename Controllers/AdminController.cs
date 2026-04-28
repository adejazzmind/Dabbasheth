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
        public AdminController(ApplicationDbContext context) => _context = context;

        private bool IsAdmin() => TempData.Peek("UserRole")?.ToString() == "Admin";

        // ── 1. DASHBOARD ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var walletTotal = await _context.Wallets.AsNoTracking().SumAsync(w => (decimal?)w.Balance) ?? 0m;
            var thriftTotal = await _context.ThriftPlans.AsNoTracking().SumAsync(p => (decimal?)p.CurrentSavings) ?? 0m;

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.AsNoTracking().CountAsync(u => u.Role == "Customer"),
                NewUsersCount = await _context.Users.AsNoTracking().CountAsync(u => u.CreatedAt >= DateTime.UtcNow.Date),
                TotalSystemBalance = walletTotal + thriftTotal,
                PendingSupportCount = await _context.SupportTickets.AsNoTracking().CountAsync(t => t.Status == "Open"),
                FlaggedTransactionCount = await _context.Transactions.AsNoTracking().CountAsync(t => t.Status == "Flagged"),
                AllUsers = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).Take(5).ToListAsync(),
                PendingTransactions = await _context.Transactions.AsNoTracking()
                                              .Where(t => t.Status == "Pending")
                                              .OrderByDescending(t => t.Date).ToListAsync(),
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                                              .Include(g => g.MemberPlans).ToListAsync()
            };
            return View(vm);
        }

        // ── 2. USER MANAGEMENT ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> UserManagement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var users = await _context.Users.AsNoTracking().OrderByDescending(u => u.CreatedAt).ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Admin/ToggleUserStatus/{id}")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                user.Status = user.Status == "Active" ? "Frozen" : "Active";
                await _context.SaveChangesAsync();
                TempData["Message"] = $"Status updated for {user.FullName}.";
            }
            return RedirectToAction(nameof(UserManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAllUsers()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var unverified = await _context.Users
                .Where(u => !u.IsVerified && u.Role == "Customer").ToListAsync();
            foreach (var u in unverified) u.IsVerified = true;
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{unverified.Count} users verified.";
            return RedirectToAction(nameof(UserManagement));
        }

        // ── 3. WALLET CONTROL ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> WalletControl(string search = "")
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var usersQuery = _context.Users.AsNoTracking()
                .Where(u => u.Role == "Customer");

            if (!string.IsNullOrWhiteSpace(search))
                usersQuery = usersQuery.Where(u => u.FullName.ToLower().Contains(search.ToLower()));

            var vm = new AdminDashboardViewModel
            {
                AllUsers = await usersQuery.OrderBy(u => u.FullName).ToListAsync(),
                AllWallets = await _context.Wallets.AsNoTracking().ToListAsync()
            };

            ViewBag.Search = search;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FundCustomerWallet(string userEmail, decimal amount, string action)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserEmail.ToLower() == userEmail.ToLower());
            if (wallet == null)
            {
                TempData["Error"] = "Wallet not found.";
                return RedirectToAction(nameof(WalletControl));
            }

            var type = action == "debit" ? "Debit" : "Credit";

            if (action == "debit" && wallet.Balance < amount)
            {
                TempData["Error"] = $"Insufficient balance. Current: ₦{wallet.Balance:N2}";
                return RedirectToAction(nameof(WalletControl));
            }

            if (action == "debit") wallet.Balance -= amount;
            else wallet.Balance += amount;

            _context.Transactions.Add(new Transaction
            {
                UserEmail = userEmail,
                Amount = amount,
                Type = type,
                Description = $"Admin {type} by CEO",
                Date = DateTime.UtcNow,
                Status = "Success"
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = $"₦{amount:N2} {type}ed to {userEmail}'s wallet.";
            return RedirectToAction(nameof(WalletControl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreditWallet(string email, decimal amount, string note)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            if (wallet == null)
            {
                TempData["Error"] = "Wallet not found for this user.";
                return RedirectToAction(nameof(WalletControl));
            }

            wallet.Balance += amount;

            _context.Transactions.Add(new Transaction
            {
                UserEmail = email,
                Type = "Credit",
                Amount = amount,
                Description = string.IsNullOrWhiteSpace(note) ? "Admin credit" : note,
                Date = DateTime.UtcNow,
                Status = "Success"
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = $"₦{amount:N2} credited to {email} successfully.";
            return RedirectToAction(nameof(WalletControl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DebitWallet(string email, decimal amount, string note)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserEmail.ToLower() == email.ToLower());
            if (wallet == null)
            {
                TempData["Error"] = "Wallet not found for this user.";
                return RedirectToAction(nameof(WalletControl));
            }

            if (wallet.Balance < amount)
            {
                TempData["Error"] = $"Insufficient balance. Current balance: ₦{wallet.Balance:N2}";
                return RedirectToAction(nameof(WalletControl));
            }

            wallet.Balance -= amount;

            _context.Transactions.Add(new Transaction
            {
                UserEmail = email,
                Type = "Debit",
                Amount = amount,
                Description = string.IsNullOrWhiteSpace(note) ? "Admin debit" : note,
                Date = DateTime.UtcNow,
                Status = "Success"
            });

            await _context.SaveChangesAsync();
            TempData["Message"] = $"₦{amount:N2} debited from {email} successfully.";
            return RedirectToAction(nameof(WalletControl));
        }

        // ── 4. PAYOUT CONTROL ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PayoutControl()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var vm = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending")
                    .OrderByDescending(t => t.Date).ToListAsync(),
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                    .Include(g => g.MemberPlans).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveAllPending()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var pending = await _context.Transactions
                .Where(t => t.Status == "Pending").ToListAsync();
            foreach (var tx in pending) { tx.Status = "Success"; tx.Date = DateTime.UtcNow; }
            await _context.SaveChangesAsync();
            TempData["Message"] = $"{pending.Count} transactions approved.";
            return RedirectToAction(nameof(PayoutControl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTransaction(int transactionId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var tx = await _context.Transactions.FindAsync(transactionId);
            if (tx != null)
            {
                tx.Status = "Success";
                tx.Date = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Payment authorised.";
            }
            return RedirectToAction(nameof(PayoutControl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclineTransaction(int transactionId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var tx = await _context.Transactions.FindAsync(transactionId);
            if (tx != null)
            {
                tx.Status = "Declined";
                tx.Date = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                TempData["Message"] = "Transaction declined.";
            }
            return RedirectToAction(nameof(PayoutControl));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessMonthlyPayout(int thriftGroupId, int memberPlanId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
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

                decimal payout = plan.ThriftGroup!.CategoryAmount * (plan.ThriftGroup.MemberPlans?.Count ?? 0);
                plan.HasCollected = true;
                plan.PayoutDate = DateTime.UtcNow;

                var wallet = await _context.Wallets
                    .FirstOrDefaultAsync(w => w.UserEmail.ToLower() == plan.UserEmail.ToLower());
                if (wallet != null) wallet.Balance += payout;

                _context.Transactions.Add(new Transaction
                {
                    UserEmail = plan.UserEmail,
                    Amount = payout,
                    Type = "Credit",
                    Description = $"Ajo Payout: {plan.ThriftGroup.GroupName} (Slot #{plan.PayoutOrder})",
                    Date = DateTime.UtcNow,
                    Status = "Success"
                });

                await _context.SaveChangesAsync();
                TempData["Message"] = $"Payout of ₦{payout:N0} released.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Payout failure: " + ex.Message;
            }
            return RedirectToAction(nameof(AjoManagement));
        }

        // ── 5. RISK CONTROL ───────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> RiskControl()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var vm = new AdminDashboardViewModel
            {
                PendingTransactions = await _context.Transactions.AsNoTracking()
                    .Where(t => t.Status == "Pending" || t.Status == "Flagged")
                    .OrderByDescending(t => t.Date).ToListAsync(),
                AllUsers = await _context.Users.AsNoTracking().ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FlagTransaction(int transactionId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var tx = await _context.Transactions.FindAsync(transactionId);
            if (tx != null)
            {
                tx.Status = "Flagged";
                await _context.SaveChangesAsync();
                TempData["Message"] = "Transaction flagged.";
            }
            return RedirectToAction(nameof(RiskControl));
        }

        // ── 6. AJO ENGINE ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> AjoManagement()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var vm = new AdminDashboardViewModel
            {
                AllThriftGroups = await _context.ThriftGroups.AsNoTracking()
                    .Include(g => g.MemberPlans)
                    .OrderByDescending(g => g.Id).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateThriftGroup(ThriftGroup group)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            ModelState.Remove("Status");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Frequency");
            ModelState.Remove("StartDate");
            group.Status = "Active";
            group.Frequency = "Monthly";
            group.CreatedAt = DateTime.UtcNow;
            group.StartDate = DateTime.UtcNow;
            _context.ThriftGroups.Add(group);
            await _context.SaveChangesAsync();
            TempData["Message"] = $"Group '{group.GroupName}' created.";
            return RedirectToAction(nameof(AjoManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMemberToGroup(int ThriftGroupId, string UserEmail, int PayoutOrder)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            _context.ThriftPlans.Add(new ThriftPlan
            {
                ThriftGroupId = ThriftGroupId,
                UserEmail = UserEmail,
                PayoutOrder = PayoutOrder,
                Title = "Ajo Slot",
                Status = "Active",
                StartDate = DateTime.UtcNow,
                Frequency = "Monthly"
            });
            await _context.SaveChangesAsync();
            TempData["Message"] = "Member added.";
            return RedirectToAction(nameof(AjoManagement));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CloseGroup(int groupId)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var g = await _context.ThriftGroups.FindAsync(groupId);
            if (g != null) { g.Status = "Closed"; await _context.SaveChangesAsync(); }
            return RedirectToAction(nameof(AjoManagement));
        }

        // ── 7. SUPPORT CENTER ─────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> SupportCenter()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var vm = new AdminDashboardViewModel
            {
                SupportTickets = await _context.SupportTickets.AsNoTracking()
                    .OrderByDescending(t => t.CreatedAt).ToListAsync(),
                AllUsers = await _context.Users.AsNoTracking()
                    .Where(u => u.Role == "Customer")
                    .OrderBy(u => u.FullName).ToListAsync()
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResolveTicket(int ticketId, string response)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var t = await _context.SupportTickets.FindAsync(ticketId);
            if (t != null)
            {
                t.AdminResponse = response;
                t.Status = "Resolved";
                await _context.SaveChangesAsync();
                TempData["Message"] = "Ticket resolved.";
            }
            return RedirectToAction(nameof(SupportCenter));
        }

        [HttpGet]
        public async Task<IActionResult> ChatWith(string customerEmail)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var adminEmail = TempData.Peek("UserEmail")?.ToString() ?? "adejazzmind@gmail.com";

            var messages = await _context.ChatMessages.AsNoTracking()
                .Where(m => (m.SenderEmail == adminEmail && m.ReceiverEmail == customerEmail) ||
                             (m.SenderEmail == customerEmail && m.ReceiverEmail == adminEmail))
                .OrderBy(m => m.SentAt).ToListAsync();

            var unread = await _context.ChatMessages
                .Where(m => m.ReceiverEmail == adminEmail &&
                             m.SenderEmail == customerEmail && !m.IsRead)
                .ToListAsync();
            unread.ForEach(m => m.IsRead = true);
            await _context.SaveChangesAsync();

            ViewBag.CustomerEmail = customerEmail;
            ViewBag.AdminEmail = adminEmail;
            ViewBag.Messages = messages;

            var customer = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == customerEmail.ToLower());
            ViewBag.CustomerName = customer?.FullName ?? customerEmail;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAdminMessage(string customerEmail, string message)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var adminEmail = TempData.Peek("UserEmail")?.ToString() ?? "adejazzmind@gmail.com";
            if (!string.IsNullOrWhiteSpace(message))
            {
                _context.ChatMessages.Add(new ChatMessage
                {
                    SenderEmail = adminEmail,
                    ReceiverEmail = customerEmail,
                    Message = message.Trim(),
                    IsAdminMessage = true,
                    SentAt = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(ChatWith), new { customerEmail });
        }

        // ── 8. SETTINGS ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View(await _context.SystemSettings.AsNoTracking().ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateFee(string key, decimal value)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            var s = await _context.SystemSettings
                .FirstOrDefaultAsync(x => x.SettingKey == key);
            if (s != null)
            {
                s.SettingValue = value;
                await _context.SaveChangesAsync();
                TempData["Message"] = $"{key} updated.";
            }
            return RedirectToAction(nameof(Settings));
        }
    }
}