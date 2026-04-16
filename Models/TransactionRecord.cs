using System;
using System.ComponentModel.DataAnnotations; // Add this using statement

namespace Dabbasheth.Models
{
    public class TransactionRecord
    {
        public int Id { get; set; }
        public string Reference { get; set; }
        public string UserEmail { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }

        // ✅ ADD THIS LINE TO FIX THE ERROR
        public string Type { get; set; } // "Credit" or "Debit"
    }
}