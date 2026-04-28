using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string AdminResponse { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";   // Open, InProgress, Resolved

        public string Priority { get; set; } = "Medium"; // Low, Medium, High

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
