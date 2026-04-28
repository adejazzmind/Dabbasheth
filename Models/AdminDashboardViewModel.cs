using System;
using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int NewUsersCount { get; set; }
        public decimal TotalSystemBalance { get; set; }
        public decimal TotalRevenueFromFees { get; set; }
        public int PendingSupportCount { get; set; }
        public int FlaggedTransactionCount { get; set; }

        public List<Transaction> PendingTransactions { get; set; } = new();
        public List<SupportTicket> SupportTickets { get; set; } = new();
        public List<ThriftGroup> AllThriftGroups { get; set; } = new();
        public List<ThriftPlan> AllThriftPlans { get; set; } = new();
        public List<User> AllUsers { get; set; } = new();
        public List<Wallet> AllWallets { get; set; } = new();
        public List<SystemSetting> SystemSettings { get; set; } = new();
    }
}
