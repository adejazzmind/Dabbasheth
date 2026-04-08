using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class ThriftPlan
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Links the thrift plan to a specific user account. 
        /// Crucial for World-Class data isolation.
        /// </summary>
        [Required]
        public string UserEmail { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } // e.g., "December Rice" or "New Laptop"

        [DataType(DataType.Currency)]
        public decimal TargetAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal CurrentSavings { get; set; }

        public string Frequency { get; set; } // Daily, Weekly, Monthly, Yearly

        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Display(Name = "Maturity Date")]
        public DateTime MaturityDate { get; set; }

        public string Status { get; set; } // Active, Completed, Broken
    }
}