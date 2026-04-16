using System;
using System.Collections.Generic;
using System.Linq;

namespace Dabbasheth.Models
{
    public class AdminDashboardViewModel
    {
        // ==========================================
        // 📊 1. ANALYTICS & REVENUE KPIs (CEO Stats)
        // ==========================================
        public int TotalUsers { get; set; }
        public decimal TotalSystemBalance { get; set; }

        // ✅ Point 9: Platform Earnings
        public decimal TotalRevenueFromFees { get; set; }

        // ==========================================
        // 🛡️ 2. EMERGENCY & ACTION QUEUES (Action Required)
        // ==========================================

        // ✅ Point 12: Pending Customer Complaints
        public int PendingSupportCount { get; set; }

        // ✅ Point 8: Suspicious Activity Monitoring
        public int FlaggedTransactionCount { get; set; }

        // Disbursement Queue
        public List<TransactionRecord> PendingTransactions { get; set; } = new();

        // Support Ticket Ledger
        public List<SupportTicket> SupportTickets { get; set; } = new();

        // ==========================================
        // 👥 3. AJO & THRIFT MANAGEMENT
        // ==========================================
        public List<ThriftGroup> AllThriftGroups { get; set; } = new();
        public List<ThriftPlan> AllThriftPlans { get; set; } = new();

        // ==========================================
        // 📋 4. IDENTITY & WALLET LEDGERS
        // ==========================================
        public List<User> AllUsers { get; set; } = new();
        public List<Wallet> AllWallets { get; set; } = new();
        public List<SystemSetting> SystemSettings { get; set; } = new();

        // ==========================================
        // 📈 5. GROWTH ANALYTICS HELPERS
        // ==========================================
        public int NewThriftGoalsCount => AllThriftPlans
            .Count(p => p.StartDate >= DateTime.UtcNow.AddDays(-1));

        public int NewUsersCount => AllUsers
            .Count(u => u.CreatedAt >= DateTime.UtcNow.AddDays(-1));
    }
}