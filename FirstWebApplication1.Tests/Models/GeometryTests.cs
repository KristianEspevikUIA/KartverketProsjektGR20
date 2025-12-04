using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class GeometryTests
    {
        // Test 1: Sjekker at geometrien er gyldig når både breddegrad og lengdegrad finnes, og ingen linje er definert
        [Fact]
        public void Geometry_IsValidPoint_WhenLatLonPresent_AndNoLine()
        {
            // Arrange: Oppretter et hinder med gyldig punkt (Latitude og Longitude)
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = null
            };

            // Act & Assert: Verifiserer at geometrien er gyldig
            Assert.True(IsValidGeometry(data));
        }

        // Test 2: Sjekker at geometrien er gyldig når LineGeoJson er satt (linjegeometri)
        [Fact]
        public void Geometry_IsValidLine_WhenLineGeoJsonIsPresent()
        {
            // Arrange: Oppretter et hinder med linjegeometri i GeoJSON-format
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = "{\"type\":\"LineString\",\"coordinates\":[[8.0,58.0],[8.1,58.1]]}"
            };

            // Act & Assert: Verifiserer at geometrien er gyldig
            Assert.True(IsValidGeometry(data));
        }

        // Test 3: Sjekker at geometrien er ugyldig når både punkt og linje mangler
        [Fact]
        public void Geometry_IsInvalid_WhenBothPointAndLineMissing()
        {
            // Arrange: Oppretter et hinder uten punkt og uten linje
            var data = new ObstacleData
            {
                Latitude = null,
                Longitude = null,
                ObstacleHeight = 100,
                LineGeoJson = null
            };

            // Act & Assert: Verifiserer at geometrien er ugyldig
            Assert.False(IsValidGeometry(data));
        }

        // Test 4: Sjekker at geometrien er gyldig når linje er tom streng, men punkt finnes
        [Fact]
        public void Geometry_IsValid_WhenLineGeoJsonIsEmpty_AndPointExists()
        {
            // Arrange: Oppretter et hinder med punkt, men tom linjestreng
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = ""
            };

            // Act & Assert: Verifiserer at geometrien er gyldig
            Assert.True(IsValidGeometry(data));
        }

        // Hjelpemetode som avgjør om geometrien er gyldig basert på punkt og/eller linje
        private bool IsValidGeometry(ObstacleData m)
        {
            bool hasPoint = m.Latitude.HasValue && m.Longitude.HasValue;
            bool hasLine = !string.IsNullOrWhiteSpace(m.LineGeoJson);

            if (hasLine) return true;
            if (hasPoint) return true;

            return false;
        }
    }
}