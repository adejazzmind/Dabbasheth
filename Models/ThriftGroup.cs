using System;
using System.Collections.Generic;

namespace Dabbasheth.Models
{
    public class ThriftGroup
    {
        public int Id { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
        public decimal CategoryAmount { get; set; }
        public decimal MonthlyContribution { get; set; }
        public int DurationMonths { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public int TotalMembers { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public List<ThriftPlan> MemberPlans { get; set; } = new();
    }
}
