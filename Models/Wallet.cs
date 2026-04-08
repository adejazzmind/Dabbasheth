using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        /* --- 1. OWNERSHIP LINK --- */
        [Required]
        [EmailAddress]
        [Display(Name = "Account Owner Email")]
        public string UserEmail { get; set; }

        /* NOTE: We removed 'Password' from here. 
           Passwords belong in the 'User' model, not the 'Wallet' model.
        */

        /* --- 2. FINANCIAL DATA --- */
        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Balance cannot be negative")]
        public decimal Balance { get; set; } = 0.00m;

        [Required]
        [StringLength(3)]
        public string Currency { get; set; } = "NGN";

        /* --- 3. AUDIT TRAIL --- */
        [Required]
        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}