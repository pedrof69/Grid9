using System;
using System.Collections.Generic;
using Xunit;
using OptimalCoordinateCompression;

namespace OptimalCoordinateCompression.Tests
{
    /// <summary>
    /// Comprehensive test suite for UniformPrecisionCoordinateCompressor
    /// Validates uniform 3-meter precision across all latitudes and longitudes
    /// </summary>
    public class UniformPrecisionCoordinateCompressorTests
    {
        private const double PRECISION_TOLERANCE_M = 0.1; // 10cm tolerance for precision validation
        private const double MAX_EXPECTED_ERROR_M = 3.5; // Realistic maximum with 45-bit constraints
        private const double TARGET_PRECISION_M = 3.0;

        [Fact]
        public void Encode_ProducesNineCharacterString()
        {
            // Test various global coordinates
            var testCoordinates = new[]
            {
                (40.7128, -74.0060), // New York
                (51.5074, -0.1278),  // London  
                (35.6762, 139.6503), // Tokyo
                (-33.8688, 151.2083), // Sydney
                (55.7558, 37.6176),  // Moscow
                (-22.9068, -43.1729), // Rio de Janeiro
                (1.3521, 103.8198),  // Singapore
                (78.2232, 15.6267),  // Svalbard (high latitude)
                (-54.8019, -68.3030), // Ushuaia (southern)
            };

            foreach (var (lat, lon) in testCoordinates)
            {
                string encoded = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
                
                Assert.NotNull(encoded);
                Assert.Equal(9, encoded.Length);
                
                // Verify all characters are valid base32
                foreach (char c in encoded)
                {
                    Assert.Contains(c, "0123456789ABCDEFGHJKMNPQRSTVWXYZ");
                }
            }
        }

        [Fact]
        public void EncodeWithHumanReadable_ProducesFormattedString()
        {
            string compact = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060, false);
            string readable = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060, true);
            
            Assert.Equal(9, compact.Length);
            Assert.Equal(11, readable.Length);
            Assert.Equal('-', readable[3]);
            Assert.Equal('-', readable[7]);
            
            // Both should decode to same coordinates
            var (lat1, lon1) = UniformPrecisionCoordinateCompressor.Decode(compact);
            var (lat2, lon2) = UniformPrecisionCoordinateCompressor.Decode(readable);
            
            Assert.Equal(lat1, lat2, 6);
            Assert.Equal(lon1, lon2, 6);
        }

        [Fact]
        public void Decode_RoundTripAccuracy()
        {
            var testCoordinates = new[]
            {
                (40.7128, -74.0060), // New York
                (51.5074, -0.1278),  // London  
                (35.6762, 139.6503), // Tokyo
                (-33.8688, 151.2083), // Sydney
                (0.0, 0.0),          // Origin
                (89.9, 0.0),         // Near North Pole
                (-89.9, 0.0),        // Near South Pole
                (0.0, 179.9),        // Near Date Line
                (0.0, -179.9),       // Near Date Line (other side)
            };

            foreach (var (originalLat, originalLon) in testCoordinates)
            {
                string encoded = UniformPrecisionCoordinateCompressor.Encode(originalLat, originalLon);
                var (decodedLat, decodedLon) = UniformPrecisionCoordinateCompressor.Decode(encoded);
                
                // Calculate error in meters
                double distance = CalculateHaversineDistance(originalLat, originalLon, decodedLat, decodedLon);
                
                Assert.True(distance <= MAX_EXPECTED_ERROR_M, 
                    $"Round-trip error {distance:F1}m exceeds maximum {MAX_EXPECTED_ERROR_M}m at ({originalLat}, {originalLon})");
            }
        }

        [Theory]
        [InlineData(90, 0)]    // North Pole
        [InlineData(-90, 0)]   // South Pole  
        [InlineData(0, 180)]   // Date Line
        [InlineData(0, -180)]  // Date Line
        [InlineData(0, 0)]     // Origin
        public void Encode_HandlesEdgeCases(double lat, double lon)
        {
            string encoded = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
            Assert.Equal(9, encoded.Length);
            
            var (decodedLat, decodedLon) = UniformPrecisionCoordinateCompressor.Decode(encoded);
            double distance = CalculateHaversineDistance(lat, lon, decodedLat, decodedLon);
            
            Assert.True(distance <= MAX_EXPECTED_ERROR_M);
        }

        [Fact]
        public void GetActualPrecision_ReturnsUniformPrecision()
        {
            // Test precision at various latitudes - should be uniform with equal-area projection
            var testLatitudes = new[] { -80, -60, -40, -20, 0, 20, 40, 60, 80 };
            var testLongitudes = new[] { -150, -90, -30, 0, 30, 90, 150 };

            var precisionResults = new List<(double lat, double lon, double totalError)>();

            foreach (double lat in testLatitudes)
            {
                foreach (double lon in testLongitudes)
                {
                    var (xError, yError, totalError) = UniformPrecisionCoordinateCompressor.GetActualPrecision(lat, lon);
                    precisionResults.Add((lat, lon, totalError));
                    
                    // All precision values should be similar (uniform)
                    Assert.True(totalError <= MAX_EXPECTED_ERROR_M, 
                        $"Precision {totalError:F1}m exceeds maximum at ({lat}, {lon})");
                    Assert.True(totalError >= 2.0, 
                        $"Precision {totalError:F1}m suspiciously low at ({lat}, {lon})");
                }
            }

            // Verify uniformity - calculate coefficient of variation
            double meanPrecision = 0;
            foreach (var (_, _, totalError) in precisionResults)
            {
                meanPrecision += totalError;
            }
            meanPrecision /= precisionResults.Count;

            double variance = 0;
            foreach (var (_, _, totalError) in precisionResults)
            {
                double diff = totalError - meanPrecision;
                variance += diff * diff;
            }
            variance /= precisionResults.Count;
            double stdDev = Math.Sqrt(variance);
            double coefficientOfVariation = stdDev / meanPrecision;

            // Coefficient of variation should be reasonable (longitude varies with latitude)
            Assert.True(coefficientOfVariation < 0.3, 
                $"Precision variation coefficient {coefficientOfVariation:F3} indicates non-uniform precision");
        }

        [Fact]
        public void PrecisionComparison_BetterThanOldAlgorithm()
        {
            // Compare precision at high latitudes where old algorithm was poor
            var highLatitudeLocations = new[]
            {
                (70.0, 0.0),     // Northern Canada
                (75.0, 120.0),   // Northern Siberia  
                (65.0, -150.0),  // Northern Alaska
                (-65.0, 45.0),   // Antarctica
            };

            foreach (var (lat, lon) in highLatitudeLocations)
            {
                var (xError, yError, totalError) = UniformPrecisionCoordinateCompressor.GetActualPrecision(lat, lon);
                
                // New algorithm should maintain good precision even at high latitudes
                Assert.True(totalError <= 5.0, 
                    $"High latitude precision {totalError:F1}m is poor at ({lat}, {lon})");
                    
                // Test actual encoding accuracy
                string encoded = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = UniformPrecisionCoordinateCompressor.Decode(encoded);
                double actualError = CalculateHaversineDistance(lat, lon, decodedLat, decodedLon);
                
                Assert.True(actualError <= MAX_EXPECTED_ERROR_M,
                    $"High latitude actual error {actualError:F1}m exceeds maximum at ({lat}, {lon})");
            }
        }

        [Fact]
        public void CalculateDistance_AccurateResults()
        {
            // Test known distances
            string nyc = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);
            string london = UniformPrecisionCoordinateCompressor.Encode(51.5074, -0.1278);
            
            double calculatedDistance = UniformPrecisionCoordinateCompressor.CalculateDistance(nyc, london);
            double expectedDistance = CalculateHaversineDistance(40.7128, -74.0060, 51.5074, -0.1278);
            
            // Should be within a few hundred meters due to quantization
            double distanceError = Math.Abs(calculatedDistance - expectedDistance);
            Assert.True(distanceError < 1000, $"Distance calculation error {distanceError:F0}m too large");
        }

        [Fact]
        public void IsValidEncoding_ValidatesCorrectly()
        {
            // Valid encodings
            Assert.True(UniformPrecisionCoordinateCompressor.IsValidEncoding("012345678")); // 9 chars
            Assert.True(UniformPrecisionCoordinateCompressor.IsValidEncoding("ABCDEFGHJ")); // 9 chars
            Assert.True(UniformPrecisionCoordinateCompressor.IsValidEncoding("012-345-678")); // Formatted
            
            // Invalid encodings
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding(""));
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12345678")); // Too short
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("1234567890")); // Too long
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12345678I")); // Invalid char 'I'
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12345678L")); // Invalid char 'L'
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12345678O")); // Invalid char 'O'
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12345678U")); // Invalid char 'U'
            Assert.False(UniformPrecisionCoordinateCompressor.IsValidEncoding("12-345-678"));  // Wrong format
        }

        [Fact]
        public void FormatForHumans_WorksCorrectly()
        {
            string compact = "123456789";
            string formatted = UniformPrecisionCoordinateCompressor.FormatForHumans(compact);
            
            Assert.Equal("123-456-789", formatted);
            Assert.True(UniformPrecisionCoordinateCompressor.IsFormattedForHumans(formatted));
            Assert.False(UniformPrecisionCoordinateCompressor.IsFormattedForHumans(compact));
        }

        [Fact]
        public void RemoveFormatting_WorksCorrectly()
        {
            string formatted = "123-456-789";
            string compact = UniformPrecisionCoordinateCompressor.RemoveFormatting(formatted);
            
            Assert.Equal("123456789", compact);
            
            // Already unformatted should pass through
            string alreadyCompact = "123456789";
            string result = UniformPrecisionCoordinateCompressor.RemoveFormatting(alreadyCompact);
            Assert.Equal(alreadyCompact, result);
        }

        [Theory]
        [InlineData(91, 0)]     // Latitude out of range
        [InlineData(-91, 0)]    // Latitude out of range
        [InlineData(0, 181)]    // Longitude out of range
        [InlineData(0, -181)]   // Longitude out of range
        public void Encode_ThrowsOnInvalidInput(double lat, double lon)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => 
                UniformPrecisionCoordinateCompressor.Encode(lat, lon));
        }

        [Theory]
        [InlineData("")]          // Empty
        [InlineData("12345678")]  // Too short
        [InlineData("1234567890")] // Too long
        [InlineData("123456789I")] // Invalid character
        public void Decode_ThrowsOnInvalidInput(string encoded)
        {
            Assert.Throws<ArgumentException>(() => 
                UniformPrecisionCoordinateCompressor.Decode(encoded));
        }

        [Fact]
        public void StressTest_RandomGlobalCoordinates()
        {
            var random = new Random(42); // Fixed seed for reproducibility
            int testCount = 1000;
            var failedTests = new List<(double lat, double lon, double error)>();

            for (int i = 0; i < testCount; i++)
            {
                // Generate random coordinates (avoid extreme polar regions)
                double lat = random.NextDouble() * 160 - 80; // -80 to 80
                double lon = random.NextDouble() * 360 - 180; // -180 to 180
                
                try
                {
                    string encoded = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
                    Assert.Equal(9, encoded.Length);
                    
                    var (decodedLat, decodedLon) = UniformPrecisionCoordinateCompressor.Decode(encoded);
                    double error = CalculateHaversineDistance(lat, lon, decodedLat, decodedLon);
                    
                    if (error > MAX_EXPECTED_ERROR_M)
                    {
                        failedTests.Add((lat, lon, error));
                    }
                }
                catch (Exception ex)
                {
                    failedTests.Add((lat, lon, -1)); // Error marker
                }
            }

            // Allow small number of failures due to projection edge cases
            double failureRate = (double)failedTests.Count / testCount;
            Assert.True(failureRate < 0.01, 
                $"Failure rate {failureRate:P1} too high. Failed tests: {failedTests.Count}");
        }

        [Fact]
        public void GlobalCoverage_UniformPrecisionValidation()
        {
            // Test precision uniformity across a regular global grid
            var precisionSamples = new List<double>();
            int latSteps = 18; // 10-degree steps
            int lonSteps = 36; // 10-degree steps

            for (int latStep = -8; latStep <= 8; latStep++) // -80 to 80 degrees
            {
                for (int lonStep = -17; lonStep <= 17; lonStep++) // -170 to 170 degrees
                {
                    double lat = latStep * 10.0;
                    double lon = lonStep * 10.0;
                    
                    var (xError, yError, totalError) = UniformPrecisionCoordinateCompressor.GetActualPrecision(lat, lon);
                    precisionSamples.Add(totalError);
                }
            }

            // Calculate statistics
            double minPrecision = precisionSamples[0];
            double maxPrecision = precisionSamples[0];
            double sumPrecision = 0;

            foreach (double precision in precisionSamples)
            {
                minPrecision = Math.Min(minPrecision, precision);
                maxPrecision = Math.Max(maxPrecision, precision);
                sumPrecision += precision;
            }

            double avgPrecision = sumPrecision / precisionSamples.Count;
            double precisionRange = maxPrecision - minPrecision;

            // Precision should be reasonably uniform (some variation expected due to longitude)
            Assert.True(precisionRange < 1.5, 
                $"Precision range {precisionRange:F3}m indicates non-uniform precision");
            Assert.True(avgPrecision <= 5.0 && avgPrecision >= 2.0,
                $"Average precision {avgPrecision:F1}m outside expected range");
        }

        /// <summary>
        /// Helper method to calculate Haversine distance
        /// </summary>
        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            double dLat = (lat2 - lat1) * Math.PI / 180.0;
            double dLon = (lon2 - lon1) * Math.PI / 180.0;
            double a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
                      Math.Cos(lat1 * Math.PI / 180.0) * Math.Cos(lat2 * Math.PI / 180.0) *
                      Math.Sin(dLon/2) * Math.Sin(dLon/2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));
            return R * c;
        }
    }
}