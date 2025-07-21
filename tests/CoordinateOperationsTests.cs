using OptimalCoordinateCompression;
using Xunit;

namespace Tests
{
    public class CoordinateOperationsTests
    {
        [Fact]
        public void BatchEncode_MultipleCoordinates_ReturnsCorrectCount()
        {
            // Arrange
            var coordinates = new[] 
            {
                (40.7128, -74.0060),
                (51.5074, -0.1278),
                (-33.8688, 151.2083)
            };

            // Act
            string[] results = CoordinateOperations.BatchEncode(coordinates);

            // Assert
            Assert.Equal(coordinates.Length, results.Length);
            foreach (string result in results)
            {
                Assert.Equal(9, result.Length);
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(result));
            }
        }

        [Fact]
        public void BatchDecode_MultipleEncodedStrings_ReturnsCorrectCount()
        {
            // Arrange
            var encoded = new[] { "234567880", "ABCDEFGHJ", "KMNPQRSTV" };

            // Act
            var results = CoordinateOperations.BatchDecode(encoded);

            // Assert
            Assert.Equal(encoded.Length, results.Length);
            foreach (var (lat, lon) in results)
            {
                Assert.InRange(lat, -80, 80);
                Assert.InRange(lon, -180, 180);
            }
        }

        [Fact]
        public void BatchEncodeDecodeRoundTrip_MaintainsPrecision()
        {
            // Arrange
            var originalCoords = new[] 
            {
                (40.7128, -74.0060),
                (51.5074, -0.1278),
                (-33.8688, 151.2083),
                (0.0, 0.0)
            };

            // Act
            string[] encoded = CoordinateOperations.BatchEncode(originalCoords);
            var decoded = CoordinateOperations.BatchDecode(encoded);

            // Assert
            for (int i = 0; i < originalCoords.Length; i++)
            {
                var (originalLat, originalLon) = originalCoords[i];
                var (decodedLat, decodedLon) = decoded[i];

                double distance = MeterBasedCoordinateCompressor.CalculateDistance(
                    MeterBasedCoordinateCompressor.Encode(originalLat, originalLon),
                    MeterBasedCoordinateCompressor.Encode(decodedLat, decodedLon)
                );

                Assert.True(distance <= 3.0, $"Precision lost in batch operation at index {i}");
            }
        }

        [Fact]
        public void FindNearby_ValidCenter_ReturnsCoordinatesWithinRadius()
        {
            // Arrange
            double centerLat = 40.7128;
            double centerLon = -74.0060;
            double radius = 100; // 100 meters

            // Act
            string[] nearby = CoordinateOperations.FindNearby(centerLat, centerLon, radius, 50);

            // Assert
            Assert.NotNull(nearby);
            Assert.True(nearby.Length > 0, "Should find some coordinates within radius");

            string centerEncoded = MeterBasedCoordinateCompressor.Encode(centerLat, centerLon);
            
            foreach (string coordinate in nearby)
            {
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(coordinate));
                
                double distance = MeterBasedCoordinateCompressor.CalculateDistance(centerEncoded, coordinate);
                Assert.True(distance <= radius, $"Found coordinate outside radius: {distance}m > {radius}m");
            }
        }

        [Fact]
        public void FindNearby_SmallRadius_ReturnsLimitedResults()
        {
            // Arrange
            double centerLat = 40.7128;
            double centerLon = -74.0060;
            double radius = 10; // Very small radius - 10 meters

            // Act
            string[] nearby = CoordinateOperations.FindNearby(centerLat, centerLon, radius, 100);

            // Assert
            Assert.NotNull(nearby);
            
            // With current precision and 10m radius, should have limited results
            Assert.True(nearby.Length <= 100, "Small radius should return limited results");
        }

        [Fact]
        public void FindNearby_MaxResultsLimit_RespectsLimit()
        {
            // Arrange
            double centerLat = 40.7128;
            double centerLon = -74.0060;
            double radius = 1000; // Large radius - 1km
            int maxResults = 10;

            // Act
            string[] nearby = CoordinateOperations.FindNearby(centerLat, centerLon, radius, maxResults);

            // Assert
            Assert.True(nearby.Length <= maxResults, "Should respect max results limit");
        }

        [Theory]
        [InlineData(0.0, 0.0)]      // Equator
        [InlineData(85.0, 0.0)]     // Near North Pole
        [InlineData(-85.0, 0.0)]    // Near South Pole
        [InlineData(40.0, 178.0)]   // Near Date Line
        public void FindNearby_BoundaryConditions_HandlesGracefully(double lat, double lon)
        {
            // Act & Assert - Should not throw exceptions
            string[] nearby = CoordinateOperations.FindNearby(lat, lon, 100, 20);
            
            Assert.NotNull(nearby);
            // May have fewer results near boundaries, but should not crash
        }
    }
}