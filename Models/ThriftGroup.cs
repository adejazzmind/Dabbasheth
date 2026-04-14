using System;
using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class ThriftGroup
    {
        public int Id { get; set; }

        // --- 🏷️ GROUP IDENTITY ---
        public string GroupName { get; set; } // e.g., "AACSL 100K Group A"
        public string Status { get; set; } // Pending, Running, Completed
        public DateTime StartDate { get; set; }

        // --- 💰 CATEGORY & FINANCIALS ---
        // This handles your 5k, 10k, 50k, 100k, to 1M tiers
        public decimal CategoryAmount { get; set; }

        // The monthly contribution from each member
        public decimal MonthlyContribution { get; set; }

        // --- 📅 CYCLE CONFIGURATION ---
        // Supports your choice of 5, 6, or minimum 10 months
        public int DurationMonths { get; set; }
        public string Frequency { get; set; } // Default: "Monthly"

        // --- 👥 MEMBERSHIP ---
        public int TotalMembers { get; set; }

        // Navigation Property: Links the individual payout slots/members to this group
        public List<ThriftPlan> MemberPlans { get; set; } = new();
    }
}