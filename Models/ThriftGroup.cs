using System;
using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class ThriftGroup
    {
        public int Id { get; set; }

        // --- 🏷️ GROUP IDENTITY ---
        public string GroupName { get; set; } // e.g., "Ogba Market Hub"
        public string Status { get; set; } = "Active"; // Active, Running, Completed, Closed

        // --- 💰 FINANCIALS ---
        public decimal CategoryAmount { get; set; }
        public decimal MonthlyContribution { get; set; }

        // --- 📅 CONFIGURATION ---
        public int DurationMonths { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public int TotalMembers { get; set; }

        // --- ⏱️ TIMESTAMPS ---
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // ✅ Added this back
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        // --- 👥 MEMBERSHIP ---
        public List<ThriftPlan> MemberPlans { get; set; } = new();
    }
}