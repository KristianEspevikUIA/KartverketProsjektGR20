using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string ReporterName { get; set; } = string.Empty;

        [StringLength(256)]
        [EmailAddress]
        public string? ReporterEmail { get; set; }

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        public DateTime SubmittedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = string.Empty;

        public int? ObstacleId { get; set; }

        public ObstacleData? Obstacle { get; set; }
    }
}
