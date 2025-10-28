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
        
        // Helper method:
        // Runs standard data annotations validation on an object,
        // similar to what ASP.NET Core model binding does at runtime.
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, results, validateAllProperties: true);
            return results;
        }

        // SECTION 1: Data annotation attribute tests
        // These tests check [Required], [Range], [StringLength], etc.

        [Fact]
        public void MissingRequiredFields_ShouldFailValidation()
        {
            // Arrange:
            var data = new ObstacleData();

            // Act:
            var results = ValidateModel(data);

            // Assert:
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleName)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleDescription)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleData.Latitude)));
            Assert.Contains(results, r => r.MemberNames.Contains(nameof(ObstacleData.Longitude)));
        }

        [Fact]
        public void HeightOutsideRange_ShouldFailRangeAttribute()
        {
            // Arrange:
            var tooLow = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Desc",
                ObstacleHeight = 5,    // < 15 (invalid)
                Latitude = 58,
                Longitude = 7
            };

            var tooHigh = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Desc",
                ObstacleHeight = 999,  // > 300 (invalid)
                Latitude = 58,
                Longitude = 7
            };

            // Act:
            var lowResults = ValidateModel(tooLow);
            var highResults = ValidateModel(tooHigh);

            // Assert:
            Assert.Contains(lowResults, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)));
            Assert.Contains(highResults, r => r.MemberNames.Contains(nameof(ObstacleData.ObstacleHeight)));
        }

        [Fact]
        public void ValidObject_WithPointLocation_ShouldPassAnnotations()
        {
            // Arrange:
            var data = new ObstacleData
            {
                ObstacleName = "Radio Tower",
                ObstacleDescription = "Red/white mast",
                ObstacleHeight = 120,
                Latitude = 58.12345,
                Longitude = 7.98765
            };

            // Act:
            var results = ValidateModel(data);

            // Assert:
            Assert.Empty(results);
        }

        [Fact]
        public void LineGeoJson_ParsesIntoLineCoordinates()
        {
            // Arrange:
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

            // Act:
            var coords = data.LineCoordinates;

            // Assert:
            Assert.Equal(3, coords.Count);
            
            Assert.Equal(58.0, coords[0].Latitude, 3);
            Assert.Equal(7.0, coords[0].Longitude, 3);
            
            Assert.Equal(58.2, coords[^1].Latitude, 3);
            Assert.Equal(7.2, coords[^1].Longitude, 3);
        }

        [Fact]
        public void Start_End_HasLine_Length_Metadata_IsCalculated()
        {
            // Arrange:
            // A simple 2-point line. This should count as a valid line.
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

            // Act:
            var start = data.StartCoordinate;
            var end = data.EndCoordinate;
            var hasLine = data.HasLine;
            var vertexCount = data.LineVertexCount;
            var lengthMeters = data.LineLengthMeters;

            // Assert:
            // StartCoordinate and EndCoordinate should not be null.
            Assert.NotNull(start);
            Assert.NotNull(end);

            // Start should match first coordinate
            Assert.Equal(58.0, start!.Latitude, 3);
            Assert.Equal(7.0, start.Longitude, 3);

            // End should match last coordinate
            Assert.Equal(58.001, end!.Latitude, 3);
            Assert.Equal(7.0, end.Longitude, 3);
            
            Assert.True(hasLine);
            Assert.Equal(2, vertexCount);
            Assert.NotNull(lengthMeters);
            Assert.True(lengthMeters!.Value > 0);
        }

        [Fact]
        public void EmptyOrNullLineGeoJson_ShouldReturnEmptyCoordinates()
        {
            // Arrange:
            // No line data was provided.
            var data = new ObstacleData
            {
                LineGeoJson = null
            };

            // Act:
            var coords = data.LineCoordinates;

            // Assert:
            Assert.NotNull(coords);
            Assert.Empty(coords);
            Assert.False(data.HasLine);
            Assert.Equal(0, data.LineVertexCount);
            Assert.Null(data.LineLengthMeters);
            Assert.Null(data.StartCoordinate);
            Assert.Null(data.EndCoordinate);
        }

        [Fact]
        public void InvalidJson_ShouldProduceEmptyCoordinates()
        {
            // Arrange:
            // LineGeoJson is not valid JSON.
            var data = new ObstacleData
            {
                LineGeoJson = "{ This is not valid JSON at all"
            };

            // Act:
            var coords = data.LineCoordinates;

            // Assert:
            // Parse should fail safely and return an empty list rather than throwing.
            Assert.NotNull(coords);
            Assert.Empty(coords);
        }

        [Fact]
        public void LineCoordinates_CachesResults()
        {
            // Arrange:
            // Create a simple valid 2-point line.
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

            // Act:
            // First access triggers parsing and caching.
            var firstCall = data.LineCoordinates;

            // Second access should return the cached list instance, not re-parse the JSON.
            var secondCall = data.LineCoordinates;

            // Assert:
            // Reference equality here means the cache was reused.
            Assert.Same(firstCall, secondCall);
        }

        [Fact]
        public void UpdatingLineGeoJson_InvalidatesCache()
        {
            // Arrange:
            // First, set some LineGeoJson and access LineCoordinates to warm the cache.
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

            // Act:
            data.LineGeoJson = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [10.0, 60.0],
                    [10.5, 60.5]
                ]
            }";

            var newCoords = data.LineCoordinates;

            // Assert:
            Assert.NotSame(oldCoords, newCoords);

            // Verify it actually parsed the new coordinates.
            Assert.Equal(60.0, newCoords[0].Latitude, 3);
            Assert.Equal(10.0, newCoords[0].Longitude, 3);
        }

        [Fact]
        public void Validate_NoLine_NoPointLocation_ShouldReturnError()
        {
            // Arrange:
            var data = new ObstacleData
            {
                ObstacleName = "Test",
                ObstacleDescription = "Test desc",
                ObstacleHeight = 50,
                LineGeoJson = null,
                Latitude = null,
                Longitude = null
            };

            // Act:
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert:
            // Expect an error that tells the user to click on the map.
            Assert.Single(results);
            Assert.Contains("Please click on the map", results[0].ErrorMessage!);
            Assert.Contains(nameof(ObstacleData.Latitude), results[0].MemberNames);
            Assert.Contains(nameof(ObstacleData.Longitude), results[0].MemberNames);
        }

        [Fact]
        public void Validate_NoLine_ButHasPointLocation_ShouldBeValid()
        {
            // Arrange:
            var data = new ObstacleData
            {
                ObstacleName = "Tower",
                ObstacleDescription = "Test desc",
                ObstacleHeight = 70,
                LineGeoJson = null,
                Latitude = 58.0,
                Longitude = 7.0
            };

            // Act:
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert:
            // No validation errors expected.
            Assert.Empty(results);
        }

        [Fact]
        public void Validate_InvalidLineJson_ShouldReturnParseError()
        {
            // Arrange:
            // LineGeoJson is present but invalid JSON.
            // The model will try to parse it, fail, and mark _lineParseFailed.
            var data = new ObstacleData
            {
                ObstacleName = "Cable",
                ObstacleDescription = "Overhead line",
                ObstacleHeight = 40,
                LineGeoJson = "NOT VALID JSON"
            };

            // Act:
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert:
            // We expect a single error about not being able to read the line.
            Assert.Single(results);
            Assert.Contains("could not read", results[0].ErrorMessage!, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(nameof(ObstacleData.LineGeoJson), results[0].MemberNames);
        }

        [Fact]
        public void Validate_LineWithOnlyOnePoint_ShouldReturnLineTooShortError()
        {
            // Arrange:
            // Valid JSON, but only one point in "coordinates".
            // According to the model, a valid line needs at least 2 points.
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

            // Act:
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert:
            // We expect a specific error saying we need at least two points.
            Assert.Single(results);
            Assert.Contains("at least two points", results[0].ErrorMessage!, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(nameof(ObstacleData.LineGeoJson), results[0].MemberNames);
        }

        [Fact]
        public void Validate_LineWithTwoPoints_ShouldBeValid()
        {
            // Arrange:
            // A valid line with two coordinate pairs.
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

            // Act:
            var context = new ValidationContext(data, null, null);
            var results = data.Validate(context).ToList();

            // Assert:
            // A 2-point line is considered valid.
            Assert.Empty(results);
        }
        

        [Fact]
        public void LineLengthMeters_ComputesTotalDistanceAlongSegments()
        {
            // Arrange:
            var json = @"{
                ""type"": ""LineString"",
                ""coordinates"": [
                    [7.0,   58.0],
                    [7.01,  58.0],
                    [7.01,  58.01]
                ]
            }";

            var data = new ObstacleData
            {
                LineGeoJson = json
            };

            // Act:
            var length = data.LineLengthMeters;

            // Assert:
            // The length should be:
            // - not null
            // - clearly > 0
            // - realistically in the hundreds of meters range
            Assert.NotNull(length);
            Assert.True(length!.Value > 0);
            Assert.True(length.Value > 500);
        }
    }
}