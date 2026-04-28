using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        /// <summary>Credit | Debit | Ajo Payout</summary>
        public string Type { get; set; } = "Credit";

        public string Description { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.UtcNow;

        /// <summary>Pending | Success | Completed | Failed | Flagged | Declined</summary>
        public string Status { get; set; } = "Pending";
    }
}