using System.ComponentModel.DataAnnotations; //gir tilgang til valideringsregler

namespace FirstWebApplication1.Models
{
    public class ObstacleData //modell som beskriver et hinder i systemet
    {
        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        public string? ObstacleName { get; set; } //navn på hinderet (obligatorisk)

        [Required(ErrorMessage = "Height is required")]
        [Range(15, 300, ErrorMessage = "Height must be between 15 and 300 meters")]
        public double? ObstacleHeight { get; set; } //høyde i meter 15-300, (obligatorisk)

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Max 1000 characters")]
        public string? ObstacleDescription { get; set; } //beskrivelse av hinder, maks 1000 tegn (obligatorisk)



        [Required]
        public double Longitude { get; set; } //lengdegrad (obligatorisk)

        [Required]

        public double Latitude { get; set; } //breddegrad (obligatorisk)

        string? GeoJson { get; set; } //geodata i GeoJSON format (valgfritt)
    }
}
