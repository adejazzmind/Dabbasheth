using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        public decimal Balance { get; set; } = 0m;

        public string Currency { get; set; } = "NGN";

        public string WalletNumber { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
