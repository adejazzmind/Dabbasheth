using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class UserDashboardViewModel
    {
        public Wallet Wallet { get; set; }
        public List<ThriftPlan> ThriftPlans { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<ThriftGroup> ThriftGroups { get; set; }
    }
}