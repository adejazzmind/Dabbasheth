using System;
using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Role { get; set; } = "Customer";

        // ✅ ADD THIS LINE TO FIX THE ERROR
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}