using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } // The critical new field for real user details

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Defines system access levels: "Admin" or "Customer"
        /// </summary>
        public string Role { get; set; }
    }
}