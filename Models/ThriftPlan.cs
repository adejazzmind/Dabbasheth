using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dabbasheth.Models
{
    public class ThriftPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        // --- 💰 FINANCIALS ---
        [DataType(DataType.Currency)]
        public decimal TargetAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentSavings { get; set; }

        [Required]
        public string Frequency { get; set; } // Daily, Weekly, Monthly, Yearly

        // --- 📅 TIMELINE & STATUS ---
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime MaturityDate { get; set; }
        public string Status { get; set; } = "Active"; // Active, Completed

        // --- 👥 AJO GROUP LOGIC (NEW) ---
        // Links the individual to a specific Ajo Cycle (Ogba Market, etc.)
        public int? ThriftGroupId { get; set; }

        // Defines the "Packing Month" (1 = Jan, 2 = Feb, etc.)
        public int PayoutOrder { get; set; }

        // Security check: Has this member packed their bulk amount yet?
        public bool HasCollected { get; set; } = false;

        [ForeignKey("ThriftGroupId")]
        public ThriftGroup? ThriftGroup { get; set; }

        // ==========================================
        // 🚀 BUSINESS LOGIC ENGINE
        // ==========================================
        public void CalculateMaturity()
        {
            this.MaturityDate = this.Frequency switch
            {
                "Daily" => DateTime.UtcNow.AddDays(30),
                "Weekly" => DateTime.UtcNow.AddDays(84), // 12 Weeks
                "Monthly" => DateTime.UtcNow.AddMonths(12),
                "Yearly" => DateTime.UtcNow.AddYears(1),
                _ => DateTime.UtcNow.AddMonths(1)
            };
        }
    }
}