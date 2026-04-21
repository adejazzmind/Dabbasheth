using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public string Type { get; set; } // Credit, Debit, Ajo Payout

        public string Description { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } // Pending, Completed, Failed
    }
}