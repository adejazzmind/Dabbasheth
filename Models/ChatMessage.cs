using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderEmail { get; set; } = string.Empty;

        [Required]
        public string ReceiverEmail { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsAdminMessage { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}