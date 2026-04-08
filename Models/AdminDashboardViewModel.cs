using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class AdminDashboardViewModel
    {
        public string OrganizationName => "IES Life Hub (Dabbasheth)";
        public string OrganizationType => "General Merchandise & Business Services";
        public int TotalUsers { get; set; }
        public decimal TotalSystemBalance { get; set; }
        public List<User> AllUsers { get; set; }
        public List<Wallet> AllWallets { get; set; }
        public List<ThriftPlan> AllThriftPlans { get; set; }
    }
}