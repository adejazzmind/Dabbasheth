using System;
using System.Collections.Generic;
using System.Linq;

namespace Dabbasheth.Models
{
    public class AdminDashboardViewModel
    {
        // ==========================================
        // 📊 1. CEO HIGH-LEVEL STATS
        // ==========================================
        public int TotalUsers { get; set; }
        public decimal TotalSystemBalance { get; set; }

        // ==========================================
        // 🛡️ 2. THE CEO HANDSHAKE (Approval Queues)
        // ==========================================
        public List<TransactionRecord> PendingTransactions { get; set; } = new();

        // ==========================================
        // 👥 3. AJO & THRIFT MANAGEMENT
        // ==========================================
        // Added to support Ogba Market and other Group Cycles
        public List<ThriftGroup> AllThriftGroups { get; set; } = new();
        public List<ThriftPlan> AllThriftPlans { get; set; } = new();

        // ==========================================
        // 📋 4. SYSTEM ACCOUNT LEDGERS
        // ==========================================
        public List<User> AllUsers { get; set; } = new();
        public List<Wallet> AllWallets { get; set; } = new();

        // ==========================================
        // 📈 5. ANALYTICS HELPERS
        // ==========================================
        // Automatically calculates goals created in the last 24 hours
        public int NewThriftGoalsCount => AllThriftPlans
            .Count(p => p.StartDate >= DateTime.UtcNow.AddDays(-1));
    }
}