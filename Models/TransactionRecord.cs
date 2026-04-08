using System;
using System.ComponentModel.DataAnnotations; // Add this using statement

namespace Dabbasheth.Models
{
    public class TransactionRecord
    {
        [Key] // This tells Neon: "This Reference string is the Unique ID"
        public string Reference { get; set; }

        public string UserEmail { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}