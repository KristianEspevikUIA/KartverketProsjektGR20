using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    /// oppsumering
    /// Tester grunnleggende regler for gyldig geometri:
    /// - Et hinder kan være et punkt (lat/lon)
    /// - Et hinder kan være en linje (LineGeoJson)
    /// - Minst én av disse må være satt
    public class GeometryTests
    {
        [Fact]
        public void Geometry_IsValidPoint_WhenLatLonPresent_AndNoLine()
        {
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = null
            };

            Assert.True(IsValidGeometry(data));
        }

        [Fact]
        public void Geometry_IsValidLine_WhenLineGeoJsonIsPresent()
        {
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = "{\"type\":\"LineString\",\"coordinates\":[[8.0,58.0],[8.1,58.1]]}"
            };

            Assert.True(IsValidGeometry(data));
        }

        [Fact]
        public void Geometry_IsInvalid_WhenBothPointAndLineMissing()
        {
            var data = new ObstacleData
            {
                Latitude = null,
                Longitude = null,
                ObstacleHeight = 100,
                LineGeoJson = null
            };

            Assert.False(IsValidGeometry(data));
        }

        [Fact]
        public void Geometry_IsValid_WhenLineGeoJsonIsEmpty_AndPointExists()
        {
            var data = new ObstacleData
            {
                Latitude = 58.0,
                Longitude = 8.0,
                ObstacleHeight = 100,
                LineGeoJson = ""
            };

            Assert.True(IsValidGeometry(data));
        }

        /// oppsumering
        /// Bestemmer om modellen representerer en gyldig geometri.
        /// En hindermodell er gyldig dersom:
        /// - LineGeoJson har innhold, eller
        /// - Latitude og Longitude begge er satt
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
