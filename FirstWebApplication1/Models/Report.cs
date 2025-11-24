using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(4000)]
        public string? Description { get; set; }

        [Required]
        public int ObstacleDataId { get; set; }

        public ObstacleData? Obstacle { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
