using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SettingKey { get; set; } // e.g., "WithdrawalFee"

        [Required]
        public decimal SettingValue { get; set; } // e.g., 100.00

        public string Description { get; set; } // e.g., "Platform charge for withdrawals"
    }
}