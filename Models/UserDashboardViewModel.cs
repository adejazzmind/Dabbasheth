using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class UserDashboardViewModel
    {
        public Wallet? Wallet { get; set; }
        public List<ThriftPlan> ThriftPlans { get; set; } = new();
        public List<Transaction> RecentTransactions { get; set; } = new();
        public List<ThriftGroup> ThriftGroups { get; set; } = new();
    }
}
