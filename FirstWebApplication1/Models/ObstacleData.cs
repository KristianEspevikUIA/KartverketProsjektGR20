using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace FirstWebApplication1.Models
{
    public class ObstacleData : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        public string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(15, 300, ErrorMessage = "Height must be between 15 and 300 meters")]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Max 1000 characters")]
        public string? ObstacleDescription { get; set; }

        [Required]
        public double? Longitude { get; set; }

        [Required]
        public double? Latitude { get; set; }

        [Column(TypeName = "longtext")]
        public string? LineGeoJson { get; set; }

        private IReadOnlyList<GeoCoordinate>? _cachedLine;
        private string? _cachedSource;
        private bool _lineParseFailed;

        [NotMapped]
        public IReadOnlyList<GeoCoordinate> LineCoordinates
        {
            get
            {
                if (LineGeoJson == _cachedSource && _cachedLine is not null)
                {
                    return _cachedLine;
                }

                if (string.IsNullOrWhiteSpace(LineGeoJson))
                {
                    _cachedSource = LineGeoJson;
                    _cachedLine = Array.Empty<GeoCoordinate>();
                    _lineParseFailed = false;
                    return _cachedLine;
                }

                _cachedSource = LineGeoJson;
                var parsed = ParseLine(LineGeoJson);
                _lineParseFailed = parsed is null;
                _cachedLine = parsed ?? Array.Empty<GeoCoordinate>();
                return _cachedLine;
            }
        }

        [NotMapped]
        public GeoCoordinate? StartCoordinate => LineCoordinates.FirstOrDefault();

        [NotMapped]
        public GeoCoordinate? EndCoordinate => LineCoordinates.LastOrDefault();

        [NotMapped]
        public bool HasLine => LineCoordinates.Count >= 2;

        [NotMapped]
        public int LineVertexCount => LineCoordinates.Count;

        [NotMapped]
        public double? LineLengthMeters => HasLine ? CalculateLineLength(LineCoordinates) : null;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _ = LineCoordinates;

            if (!_lineParseFailed && string.IsNullOrWhiteSpace(LineGeoJson))
            {
                if (!Latitude.HasValue || !Longitude.HasValue)
                {
                    yield return new ValidationResult(
                        "Please click on the map to choose a location.",
                        new[] { nameof(Latitude), nameof(Longitude) });
                }
                yield break;
            }

            if (_lineParseFailed)
            {
                yield return new ValidationResult(
                    "We could not read the drawn line. Please try drawing it again.",
                    new[] { nameof(LineGeoJson) });
                yield break;
            }

            if (!HasLine)
            {
                yield return new ValidationResult(
                    "A line must contain at least two points.",
                    new[] { nameof(LineGeoJson) });
            }
        }

        private static IReadOnlyList<GeoCoordinate>? ParseLine(string geoJson)
        {
            try
            {
                using var document = JsonDocument.Parse(geoJson);
                var root = document.RootElement;

                if (!root.TryGetProperty("type", out var typeProperty) ||
                    !string.Equals(typeProperty.GetString(), "LineString", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (!root.TryGetProperty("coordinates", out var coordinatesProperty) ||
                    coordinatesProperty.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                var points = new List<GeoCoordinate>();

                foreach (var coordinate in coordinatesProperty.EnumerateArray())
                {
                    if (coordinate.ValueKind != JsonValueKind.Array || coordinate.GetArrayLength() < 2)
                    {
                        continue;
                    }

                    var longitude = coordinate[0].GetDouble();
                    var latitude = coordinate[1].GetDouble();
                    points.Add(new GeoCoordinate(latitude, longitude));
                }

                return points;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static double CalculateLineLength(IReadOnlyList<GeoCoordinate> coordinates)
        {
            double total = 0;
            for (var i = 1; i < coordinates.Count; i++)
            {
                total += HaversineDistance(coordinates[i - 1], coordinates[i]);
            }

            return total;
        }

        private static double HaversineDistance(GeoCoordinate first, GeoCoordinate second)
        {
            const double EarthRadius = 6371000; // meters

            var firstLat = DegreesToRadians(first.Latitude);
            var secondLat = DegreesToRadians(second.Latitude);
            var deltaLat = DegreesToRadians(second.Latitude - first.Latitude);
            var deltaLng = DegreesToRadians(second.Longitude - first.Longitude);

            var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                    Math.Cos(firstLat) * Math.Cos(secondLat) *
                    Math.Sin(deltaLng / 2) * Math.Sin(deltaLng / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadius * c;
        }

        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;

        public record GeoCoordinate(double Latitude, double Longitude);
    }
}
