using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication1.Models
{
    public class ObstacleData
    {
        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        public string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(16, 300, ErrorMessage = "Height must be between 15 and 300 meters")]
        public double? ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Max 1000 characters")]
        public string? ObstacleDescription { get; set; }
    }
}
