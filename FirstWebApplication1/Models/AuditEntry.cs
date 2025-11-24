using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class AuditEntry
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [Required]
        public int EntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [StringLength(256)]
        public string? PerformedBy { get; set; }

        public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

        [StringLength(4000)]
        public string? Changes { get; set; }
    }
}
