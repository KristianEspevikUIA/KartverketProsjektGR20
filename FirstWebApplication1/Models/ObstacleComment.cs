using System;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class ObstacleComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObstacleDataId { get; set; }

        public ObstacleData? Obstacle { get; set; }

        [Required]
        [StringLength(1000)]
        public string CommentText { get; set; } = string.Empty;

        [StringLength(256)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
