using System;
using System.ComponentModel.DataAnnotations;

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

        [DataType(DataType.Currency)]
        public decimal TargetAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentSavings { get; set; }

        [Required]
        public string Frequency { get; set; } // Daily, Weekly, Monthly, Yearly

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime MaturityDate { get; set; }

        public string Status { get; set; } = "Active"; // Active, Completed

        // Logic to calculate maturity based on a standard 12-cycle savings goal
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