using System.ComponentModel.DataAnnotations;

namespace Dabbasheth.Models
{
    public class SystemSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SettingKey { get; set; } = string.Empty;

        [Required]
        public decimal SettingValue { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}
