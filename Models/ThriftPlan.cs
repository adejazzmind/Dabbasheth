using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dabbasheth.Models
{
    public class ThriftPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }
        public decimal CurrentSavings { get; set; }

        [Required]
        public string Frequency { get; set; } = "Monthly";

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime MaturityDate { get; set; }
        public string Status { get; set; } = "Active";

        public int? ThriftGroupId { get; set; }
        public int PayoutOrder { get; set; }
        public bool HasCollected { get; set; } = false;
        public DateTime? PayoutDate { get; set; }

        [ForeignKey("ThriftGroupId")]
        public ThriftGroup? ThriftGroup { get; set; }
    }
}
