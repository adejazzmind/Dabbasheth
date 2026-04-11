using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; }

        public decimal Balance { get; set; }

        public string Currency { get; set; } = "NGN";

        // ✅ ADD THESE TWO LINES TO FIX THE ERRORS
        public string WalletNumber { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}