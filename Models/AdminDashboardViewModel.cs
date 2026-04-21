using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        // ✅ Added this to fix the 'NewUsersCount' error
        public int NewUsersCount { get; set; }
        public decimal TotalSystemBalance { get; set; }
        public int PendingSupportCount { get; set; }
        public int FlaggedTransactionCount { get; set; }

        public List<User> AllUsers { get; set; }
        // ✅ Ensure this uses Transaction
        public List<Transaction> PendingTransactions { get; set; }
        public List<ThriftGroup> AllThriftGroups { get; set; }
        public List<SupportTicket> SupportTickets { get; set; }
    }
}