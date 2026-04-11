using System.Collections.Generic;

namespace Dabbasheth.Models
{
    /// <summary>
    /// The Executive ViewModel for the MD/CEO Command Center.
    /// Consolidated data for real-time system liquidity and user management.
    /// </summary>
    public class AdminDashboardViewModel
    {
        // --- 1. FIRM IDENTITY ---
        public string OrganizationName => "IES Life Hub (Dabbasheth)";
        public string OrganizationType => "General Merchandise & Business Services";

        // --- 2. KPI METRICS ---
        public int TotalUsers { get; set; }

        /// <summary>
        /// The Grand Total Liquidity (Wallets + Thrift Plans combined)
        /// </summary>
        public decimal TotalSystemBalance { get; set; }

        // --- 3. DATA REPOSITORIES ---
        // Required for the Customer Ledger table
        public List<User> AllUsers { get; set; } = new();

        // Required for the 'Wallets' breakdown badge
        public List<Wallet> AllWallets { get; set; } = new();

        // Required for the 'Thrift' breakdown badge
        public List<ThriftPlan> AllThriftPlans { get; set; } = new();
    }
}