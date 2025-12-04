using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FirstWebApplication1.Models;
using Xunit;

namespace FirstWebApplication1.Tests.Models
{
    public class ObstacleDataTests
    {
        // Hjelpemetode: Utfører standard validering basert på Data Annotations
        // tilsvarende det som skjer automatisk i ASP.NET Core ved modellbinding
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            return results;
        }

        // Test 1: Sjekker at høyde utenfor gyldig område feiler [Range]-attributtet
        [Fact]
        public void HeightOutsideRange_ShouldFailRangeAttribute()
        {
            // Arrange: Ett for lavt og ett for høyt hinder
            var tooLow = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Desc",
                ObstacleHeight = 5,    // For lav verdi (ugyldig)
                Latitude = 58,
                Longitude = 7
            };

            var tooHigh = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Desc",
                ObstacleHeight = 999,  // For høy verdi (ugyldig)
                Latitude = 58,
                Longitude = 7
            };

            // Act: Kjører validering
            var lowResults = ValidateModel(tooLow);
            var highResults = ValidateModel(tooHigh);

            // Assert: Forventer valideringsfeil på ObstacleHeight
            Assert.Contains(lowResults, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)));
            Assert.Contains(highResults, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)));
        }

        // Test 2: Sjekker at et gyldig objekt med punkt-posisjon består validering
        [Fact]
        public void ValidObject_WithPointLocation_ShouldPassAnnotations()
        {
            // Arrange: Gyldig hinder med koordinater
            var data = new ObstacleData
            {
                ObstacleName = "Radio Tower",
                ObstacleDescription = "Red/white mast",
                ObstacleHeight = 120,
                Latitude = 58.12345,
                Longitude = 7.98765
            };

            // Act: Kjører validering
            var results = ValidateModel(data);

            // Assert: Forventer ingen valideringsfeil
            Assert.Empty(results);
        }

        // Test 3: Sjekker at LineGeoJson parses korrekt til koordinater
        [Fact]
        public void LineGeoJson_ParsesIntoLineCoordinates()
        {
            // Arrange: Gyldig GeoJSON med tre punkter
            var json = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0, 58.0],
                    [7.1, 58.1],
                    [7.2, 58.2]
                ]
            }";

            var data = new ObstacleData
            {
                LineGeoJson = json
            };

            // Act: Henter ut beregnede koordinater
            var coords = data.LineCoordinates;

            // Assert: Sjekker at riktig antall og riktige verdier er lest inn
            Assert.Equal(3, coords.Count);

            Assert.Equal(58.0, coords[0].Latitude, 3);
            Assert.Equal(7.0, coords[0].Longitude, 3);

            Assert.Equal(58.2, coords[^1].Latitude, 3);
            Assert.Equal(7.2, coords[^1].Longitude, 3);
        }

        // Test 4: Sjekker at start, slutt, lengde og metadata beregnes korrekt
        [Fact]
        public void Start_End_HasLine_Length_Metadata_IsCalculated()
        {
            // Arrange: En linje med nøyaktig to punkter
            var json = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0, 58.0],
                    [7.0, 58.001]
                ]
            }";

            var data = new ObstacleData
            {
                LineGeoJson = json
            };

            // Act: Leser av beregnede verdier
            var start = data.StartCoordinate;
            var end = data.EndCoordinate;
            var hasLine = data.HasLine;
            var vertexCount = data.LineVertexCount;
            var lengthMeters = data.LineLengthMeters;

            // Assert: Kontrollerer at alt er korrekt beregnet
            Assert.NotNull(start);
            Assert.NotNull(end);

            Assert.Equal(58.0, start!.Latitude, 3);
            Assert.Equal(7.0, start.Longitude, 3);

            Assert.Equal(58.001, end!.Latitude, 3);
            Assert.Equal(7.0, end.Longitude, 3);

            Assert.True(hasLine);
            Assert.Equal(2, vertexCount);
            Assert.NotNull(lengthMeters);
            Assert.True(lengthMeters!.Value > 0);
        }

        // Test 5: Sjekker at tom eller null LineGeoJson gir tom koordinatliste
        [Fact]
        public void EmptyOrNullLineGeoJson_ShouldReturnEmptyCoordinates()
        {
            // Arrange: Ingen linje er angitt
            var data = new ObstacleData
            {
                LineGeoJson = null
            };

            // Act: Leser av koordinater
            var coords = data.LineCoordinates;

            // Assert: Forventer ingen linjeinformasjon
            Assert.NotNull(coords);
            Assert.Empty(coords);
            Assert.False(data.HasLine);
            Assert.Equal(0, data.LineVertexCount);
            Assert.Null(data.LineLengthMeters);
            Assert.Null(data.StartCoordinate);
            Assert.Null(data.EndCoordinate);
        }

        // Test 6: Sjekker at ugyldig JSON håndteres trygt uten krasj
        [Fact]
        public void InvalidJson_ShouldProduceEmptyCoordinates()
        {
            // Arrange: Ugyldig JSON-streng
            var data = new ObstacleData
            {
                LineGeoJson = "{ This is not valid JSON at all"
            };

            // Act: Forsøker å lese koordinater
            var coords = data.LineCoordinates;

            // Assert: Forventer tom liste og ingen krasj
            Assert.NotNull(coords);
            Assert.Empty(coords);
        }

        // Test 7: Sjekker at LineCoordinates caches resultatet
        [Fact]
        public void LineCoordinates_CachesResults()
        {
            // Arrange: Gyldig 2-punkts linje
            var json = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0, 58.0],
                    [7.1, 58.1]
                ]
            }";

            var data = new ObstacleData
            {
                LineGeoJson = json
            };

            // Act: Henter koordinater to ganger
            var firstCall = data.LineCoordinates;
            var secondCall = data.LineCoordinates;

            // Assert: Samme referanse brukes (cache)
            Assert.Same(firstCall, secondCall);
        }

        // Test 8: Sjekker at cache nullstilles når LineGeoJson endres
        [Fact]
        public void UpdatingLineGeoJson_InvalidatesCache()
        {
            // Arrange: Første linje
            var data = new ObstacleData
            {
                LineGeoJson = @"{
                    ""type"": ""LineString"",
                    ""coordinates"": [
                        [7.0, 58.0],
                        [7.1, 58.1]
                    ]
                }"
            };

            var oldCoords = data.LineCoordinates;

            // Act: Setter ny GeoJSON-linje
            data.LineGeoJson = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [10.0, 60.0],
                    [10.5, 60.5]
                ]
            }";

            var newCoords = data.LineCoordinates;

            // Assert: Ny cache brukes og nye koordinater stemmer
            Assert.NotSame(oldCoords, newCoords);
            Assert.Equal(60.0, newCoords[0].Latitude, 3);
            Assert.Equal(10.0, newCoords[0].Longitude, 3);
        }
        
        // Test 9: Sjekker at manglende punkt og linje gir valideringsfeil
        [Fact]
        public void Validate_NoLine_NoPointLocation_ShouldReturnError()
        {
            // Arrange: Ingen punkt eller linje
            var data = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Test desc",
                ObstacleHeight = 50,
                LineGeoJson = null,
                Latitude = null,
                Longitude = null
            };

            // Act: Kjører egenvalidering
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert: Forventer én feilmelding med veiledning
            Assert.Single(results);
            Assert.Contains("Please click on the map", results[0].ErrorMessage!);
            Assert.Contains(nameof(ObstacleData.Latitude), results[0].MemberNames);
            Assert.Contains(nameof(ObstacleData.Longitude), results[0].MemberNames);
        }

        // Test 10: Sjekker at punkt uten linje er gyldig
        [Fact]
        public void Validate_NoLine_ButHasPointLocation_ShouldBeValid()
        {
            // Arrange: Kun punkt er gitt
            var data = new ObstacleData
            {
                ObstacleName = "Tower",
                ObstacleDescription = "Test desc",
                ObstacleHeight = 70,
                LineGeoJson = null,
                Latitude = 58.0,
                Longitude = 7.0
            };

            // Act: Kjører validering
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert: Forventer ingen feil
            Assert.Empty(results);
        }

        // Test 11: Sjekker at ugyldig linje-JSON gir feilmelding
        [Fact]
        public void Validate_InvalidLineJson_ShouldReturnParseError()
        {
            // Arrange: Ugyldig GeoJSON
            var data = new ObstacleData
            {
                ObstacleName = "Cable",
                ObstacleDescription = "Overhead line",
                ObstacleHeight = 40,
                LineGeoJson = "NOT VALID JSON"
            };

            // Act: Kjører validering
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert: Forventer feilmelding om at linjen ikke kan leses
            Assert.Single(results);
            Assert.Contains("could not read", results[0].ErrorMessage!, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(nameof(ObstacleData.LineGeoJson), results[0].MemberNames);
        }

        // Test 12: Sjekker at linje med kun ett punkt gir feilmelding
        [Fact]
        public void Validate_LineWithOnlyOnePoint_ShouldReturnLineTooShortError()
        {
            // Arrange: Linje med kun ett punkt
            var onePointJson = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0, 58.0]
                ]
            }";

            var data = new ObstacleData
            {
                ObstacleName = "Cable",
                ObstacleDescription = "Hanging cable",
                ObstacleHeight = 20,
                LineGeoJson = onePointJson
            };

            // Act: Kjører validering
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert: Forventer feil om for kort linje
            Assert.Single(results);
            Assert.Contains("at least two points", results[0].ErrorMessage!, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(nameof(ObstacleData.LineGeoJson), results[0].MemberNames);
        }

        // Test 13: Sjekker at linje med to punkter er gyldig
        [Fact]
        public void Validate_LineWithTwoPoints_ShouldBeValid()
        {
            // Arrange: Gyldig linje
            var twoPointJson = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0, 58.0],
                    [7.1, 58.1]
                ]
            }";

            var data = new ObstacleData
            {
                ObstacleName = "Cable",
                ObstacleDescription = "Valid cable",
                ObstacleHeight = 20,
                LineGeoJson = twoPointJson
            };

            // Act: Kjører validering
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert: Forventer ingen feil
            Assert.Empty(results);
        }
    }
}