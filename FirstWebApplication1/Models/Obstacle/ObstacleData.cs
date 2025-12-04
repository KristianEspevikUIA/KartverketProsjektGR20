using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;

namespace FirstWebApplication1.Models
{
    public class ObstacleData : IValidatableObject // Gir mulighet for custom server-side validering
    {
        [Key]
        public int Id { get; set; }

        [StringLength(50)]
        [MaxLength(300, ErrorMessage = "Description cannot exceed 300 characters.")]
        public string? ObstacleType { get; set; }

        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100, ErrorMessage = "Max 100 characters")]
        public string? ObstacleName { get; set; }

        [Required(ErrorMessage = "Height is required")]
        [Range(15, 300, ErrorMessage = "Height must be between 15 and 300 meters")]
        public double ObstacleHeight { get; set; }

        // UI-helper: holder høyde både i meter og fot uten at databasen påvirkes
        [NotMapped]
        public double ObstacleHeightInFeet
        {
            get => ObstacleHeight * 3.28084; // Måling for visning, avhengig av rolle
            set => ObstacleHeight = value / 3.28084;
        }

        [StringLength(1000, ErrorMessage = "Max 1000 characters")]
        public string? ObstacleDescription { get; set; }

        [Required]
        public double? Longitude { get; set; }

        [Required]
        public double? Latitude { get; set; }

        [Column(TypeName = "longtext")] // GeoJSON kan være svært lange strenger
        public string? LineGeoJson { get; set; }

        // ------------------------- CACHING AV GEOJSON -------------------------
        // Når LineGeoJson ikke endres, vil LineCoordinates bruke dette cachen
        // for å unngå å parse GeoJSON gjentatte ganger (ytelsesoptimalisering)
        private IReadOnlyList<GeoCoordinate>? _cachedLine;
        private string? _cachedSource;
        private bool _lineParseFailed;

        [NotMapped]
        public IReadOnlyList<GeoCoordinate> LineCoordinates
        {
            get
            {
                // Returnerer cache hvis GeoJSON ikke har endret seg
                if (LineGeoJson == _cachedSource && _cachedLine is not null)
                {
                    return _cachedLine;
                }

                // Tom streng → ingen linje, men ikke feil
                if (string.IsNullOrWhiteSpace(LineGeoJson))
                {
                    _cachedSource = LineGeoJson;
                    _cachedLine = Array.Empty<GeoCoordinate>();
                    _lineParseFailed = false;
                    return _cachedLine;
                }

                // Parse ny GeoJSON
                _cachedSource = LineGeoJson;
                var parsed = ParseLine(LineGeoJson);
                _lineParseFailed = parsed is null; // Marker om parsing feilet
                _cachedLine = parsed ?? Array.Empty<GeoCoordinate>();
                return _cachedLine;
            }
        }

        // ------------------------- LINJEINFO (for kartet) -------------------------

        [NotMapped]
        public GeoCoordinate? StartCoordinate => LineCoordinates.FirstOrDefault(); // Første punkt

        [NotMapped]
        public GeoCoordinate? EndCoordinate => LineCoordinates.LastOrDefault(); // Siste punkt

        [NotMapped]
        public bool HasLine => LineCoordinates.Count >= 2; // Gyldig linje krever 2 punkt

        [NotMapped]
        public int LineVertexCount => LineCoordinates.Count; // Antall koordinater i linjen

        [NotMapped]
        public double? LineLengthMeters => HasLine ? CalculateLineLength(LineCoordinates) : null; // Lengde i meter

        // ------------------------- METADATA / HISTORIKK -------------------------
        // Disse feltene brukes i godkjenning, revisjon og audit trail.

        public string Status { get; set; } = string.Empty;
        public string SubmittedBy { get; set; } = string.Empty;
        public string? Organization { get; set; } // Hentes fra brukerens claim
        public DateTime SubmittedDate { get; set; }
        public string LastModifiedBy { get; set; } = string.Empty;
        public DateTime LastModifiedDate { get; set; }
        public string ApprovedBy { get; set; } = string.Empty;
        public DateTime ApprovedDate { get; set; }
        public string? DeclineReason { get; set; }
        public string DeclinedBy { get; set; } = string.Empty;
        public DateTime DeclinedDate { get; set; }

        // ------------------------- SERVER-SIDE VALIDATION -------------------------

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            _ = LineCoordinates; // Trigger parsing før validering

            // Punkt-hindre uten linje krever koordinater via kartklikk
            if (!_lineParseFailed && string.IsNullOrWhiteSpace(LineGeoJson))
            {
                if (!Latitude.HasValue || !Longitude.HasValue)
                {
                    yield return new ValidationResult(
                        "Please click on the map to choose a location.",
                        new[] { nameof(Latitude), nameof(Longitude) });
                }
                yield break; // Ikke sjekk resten, dette er punktmodus
            }

            // Ugyldig linje-GeoJSON
            if (_lineParseFailed)
            {
                yield return new ValidationResult(
                    "We could not read the drawn line. Please try drawing it again.",
                    new[] { nameof(LineGeoJson) });
                yield break;
            }

            // Linje må ha minst to koordinater
            if (!HasLine)
            {
                yield return new ValidationResult(
                    "A line must contain at least two points.",
                    new[] { nameof(LineGeoJson) });
            }
        }

        // ------------------------- GEOJSON PARSING -------------------------

        private static IReadOnlyList<GeoCoordinate>? ParseLine(string geoJson)
        {
            try
            {
                using var document = JsonDocument.Parse(geoJson);
                var root = document.RootElement;

                // Sjekker at riktig GeoJSON-type
                if (!root.TryGetProperty("type", out var typeProperty) ||
                    !string.Equals(typeProperty.GetString(), "LineString", StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // Koordinater må eksistere som liste
                if (!root.TryGetProperty("coordinates", out var coordinatesProperty) ||
                    coordinatesProperty.ValueKind != JsonValueKind.Array)
                {
                    return null;
                }

                var points = new List<GeoCoordinate>();

                // Henter hvert koordinatpar fra GeoJSON-arrayet
                foreach (var coordinate in coordinatesProperty.EnumerateArray())
                {
                    if (coordinate.ValueKind != JsonValueKind.Array || coordinate.GetArrayLength() < 2)
                    {
                        continue; // Hopper over ugyldige entries
                    }

                    var longitude = coordinate[0].GetDouble();
                    var latitude = coordinate[1].GetDouble();
                    points.Add(new GeoCoordinate(latitude, longitude));
                }

                return points;
            }
            catch (JsonException)
            {
                // Feil i JSON-formatet, typisk ved ødelagt GeoJSON-string
                return null;
            }
        }

        // ------------------------- LINJEBEREGNING -------------------------

        private static double CalculateLineLength(IReadOnlyList<GeoCoordinate> coordinates)
        {
            // Summerer avstand mellom hvert segment i linjen
            double total = 0;
            for (var i = 1; i < coordinates.Count; i++)
            {
                total += HaversineDistance(coordinates[i - 1], coordinates[i]);
            }

            return total;
        }

        // Haversine-formelen: nøyaktig avstand mellom to punkt på jordkloden
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

        public record GeoCoordinate(double Latitude, double Longitude); // Enkel koordinatmodell
    }
}
