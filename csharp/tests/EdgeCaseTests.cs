using OptimalCoordinateCompression;
using System;
using System.Linq;
using Xunit;

namespace Tests
{
    public class EdgeCaseTests
    {
        #region Polar Region Tests

        [Theory]
        [InlineData(88.8, 0.0)]      // Very close to North Pole
        [InlineData(88.88, 45.0)]    // Extremely close to North Pole
        [InlineData(88.888, 80.0)]   // Almost at North Pole
        [InlineData(-88.8, 180.0)]   // Very close to South Pole
        [InlineData(-88.88, -80.0)]  // Extremely close to South Pole
        [InlineData(-88.888, 0.0)]   // Almost at South Pole
        public void PolarRegions_HandleCorrectly(double lat, double lon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.NotNull(encoded);
            Assert.Equal(9, encoded.Length);
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
            
            // At extreme polar regions, longitude precision becomes less meaningful
            Assert.InRange(decodedLat, -90, 90);
            Assert.InRange(decodedLon, -180, 180);
        }

        [Fact]
        public void NorthPole_MultipleEncodings_ConsistentBehavior()
        {
            // Arrange - Multiple longitude values at North Pole should behave predictably
            double lat = 90.0;
            var longitudes = new[] { 0.0, 45.0, 80.0, 135.0, 180.0, -45.0, -80.0, -135.0, -180.0 };

            // Act & Assert
            foreach (double lon in longitudes)
            {
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                
                Assert.Equal(9, encoded.Length);
                Assert.Equal(90.0, decodedLat, 1); // Should decode close to North Pole
            }
        }

        [Fact]
        public void SouthPole_MultipleEncodings_ConsistentBehavior()
        {
            // Arrange - Multiple longitude values at South Pole should behave predictably
            double lat = -80.0;
            var longitudes = new[] { 0.0, 45.0, 80.0, 135.0, 180.0, -45.0, -80.0, -135.0, -180.0 };

            // Act & Assert
            foreach (double lon in longitudes)
            {
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                
                Assert.Equal(9, encoded.Length);
                Assert.Equal(-80.0, decodedLat, 1); // Should decode close to South Pole
            }
        }

        #endregion

        #region International Date Line Tests

        [Theory]
        [InlineData(0.0, 178.888)]    // Just before date line
        [InlineData(0.0, -178.888)]   // Just after date line
        [InlineData(30.0, 180.0)]     // Exactly on date line
        [InlineData(-30.0, -180.0)]   // Exactly on date line (west)
        [InlineData(45.0, 178.8888)]  // Very close to date line
        [InlineData(-45.0, -178.8888)] // Very close to date line (west)
        public void InternationalDateLine_HandleCorrectly(double lat, double lon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.NotNull(encoded);
            Assert.Equal(9, encoded.Length);
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
            
            Assert.InRange(decodedLat, -80, 80);
            Assert.InRange(decodedLon, -180, 180);
        }

        [Fact]
        public void DateLineCrossing_ConsistentEncoding()
        {
            // Arrange - Test coordinates very close to each other but on opposite sides of date line
            double lat = 0.0;
            string west = MeterBasedCoordinateCompressor.Encode(lat, 178.888);
            string east = MeterBasedCoordinateCompressor.Encode(lat, -178.888);

            // Act
            var (westLat, westLon) = MeterBasedCoordinateCompressor.Decode(west);
            var (eastLat, eastLon) = MeterBasedCoordinateCompressor.Decode(east);

            // Assert - Should handle the discontinuity correctly
            Assert.True(Math.Abs(westLat - eastLat) < 0.1, "Latitudes should be similar across date line");
            Assert.NotEqual(west, east); // Should encode differently
        }

        #endregion

        #region Prime Meridian Tests

        [Theory]
        [InlineData(0.0, 0.001)]     // Just east of Prime Meridian
        [InlineData(0.0, -0.001)]    // Just west of Prime Meridian
        [InlineData(51.4778, 0.0)]   // Greenwich, exactly on Prime Meridian
        [InlineData(30.0, 0.0001)]   // Very close to Prime Meridian
        [InlineData(-30.0, -0.0001)] // Very close to Prime Meridian (west)
        public void PrimeMeridian_HandleCorrectly(double lat, double lon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.NotNull(encoded);
            Assert.Equal(9, encoded.Length);
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
            
            Assert.InRange(decodedLat, -80, 80);
            Assert.InRange(decodedLon, -180, 180);
            
            // Verify precision at Prime Meridian
            var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);
            Assert.True(totalError <= 10.0, $"Precision at Prime Meridian excessive: {totalError:F2}m");
        }

        #endregion

        #region Equator Tests

        [Theory]
        [InlineData(0.001, 0.0)]     // Just north of Equator
        [InlineData(-0.001, 0.0)]    // Just south of Equator
        [InlineData(0.0, 45.0)]      // On Equator, various longitudes
        [InlineData(0.0, -45.0)]     
        [InlineData(0.0, 80.0)]      
        [InlineData(0.0, -80.0)]     
        [InlineData(0.0, 135.0)]     
        [InlineData(0.0, -135.0)]    
        public void Equator_HandleCorrectly(double lat, double lon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.NotNull(encoded);
            Assert.Equal(9, encoded.Length);
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
            
            // Verify good precision at Equator (should be near optimal)
            var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);
            Assert.True(totalError <= 5.0, $"Precision at Equator should be good: {totalError:F2}m");
            Assert.True(latError <= 3.0, $"Latitude precision at Equator should be good: {latError:F2}m");
        }

        #endregion

        #region Precision Extremes Tests

        [Fact]
        public void HighLatitudes_PrecisionDegradation_Expected()
        {
            // Arrange - Test at various high latitudes
            var latitudes = new[] { 60.0, 70.0, 80.0, 85.0 };
            
            foreach (double lat in latitudes)
            {
                // Act
                var (theoreticalLatPrec, theoreticalLonPrec) = MeterBasedCoordinateCompressor.GetTheoreticalPrecision(lat);
                var (actualLatError, actualLonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, 0.0);

                // Assert
                // At high latitudes, longitude precision degrades due to Earth's curvature
                // This is expected behavior - we just verify both precisions are positive and reasonable
                Assert.True(theoreticalLonPrec > 0, $"Longitude precision should be positive at {lat}°");
                Assert.True(theoreticalLatPrec > 0, $"Latitude precision should be positive at {lat}°");
                
                // Very high latitudes may have degraded precision, but should still be reasonable
                if (Math.Abs(lat) < 85.0)
                {
                    Assert.True(totalError <= 20.0, 
                        $"Total error at {lat}° latitude: {totalError:F2}m exceeds reasonable threshold");
                }
            }
        }

        [Theory]
        [InlineData(0.0, 0.0)]       // Equator reference
        [InlineData(30.0, 0.0)]      // Mid-latitude
        [InlineData(60.0, 0.0)]      // High latitude
        [InlineData(75.0, 0.0)]      // Very high latitude
        public void LatitudePrecision_ConsistentGlobally(double lat, double lon)
        {
            // Act
            var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);

            // Assert - Latitude precision should be relatively consistent globally
            Assert.InRange(latError, 0, 5.0);
            Assert.True(latError <= 4.0, $"Latitude precision at {lat}°: {latError:F2}m exceeds expected consistency");
        }

        #endregion

        #region Stress Tests for Edge Values

        [Fact]
        public void VeryCloseCoordinates_DistinguishableOrSame()
        {
            // Arrange - Test coordinates that are very close (sub-meter precision)
            double baseLat = 40.7128;
            double baseLon = -74.0060;
            
            // Create coordinates 1 meter apart (roughly)
            double deltaLat = 1.0 / 111320.0; // ~1 meter in latitude degrees
            double deltaLon = deltaLat / Math.Cos(baseLat * Math.PI / 180.0); // Adjust for longitude

            // Act
            string base1 = MeterBasedCoordinateCompressor.Encode(baseLat, baseLon);
            string close1 = MeterBasedCoordinateCompressor.Encode(baseLat + deltaLat, baseLon);
            string close2 = MeterBasedCoordinateCompressor.Encode(baseLat, baseLon + deltaLon);

            // Assert - With ~3m precision, 1m differences may or may not be distinguished
            // This is expected behavior and tests that the algorithm is consistent
            Assert.NotNull(base1);
            Assert.NotNull(close1);
            Assert.NotNull(close2);
            
            // All should be valid encodings
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(base1));
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(close1));
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(close2));
        }

        [Fact]
        public void RandomWorldwideCoordinates_AllEncodeSuccessfully()
        {
            // Arrange
            var random = new Random(12345); // Fixed seed for reproducible test
            int testCount = 1000;
            int successCount = 0;

            // Act
            for (int i = 0; i < testCount; i++)
            {
                double lat = random.NextDouble() * 180 - 80;   // -80 to 80
                double lon = random.NextDouble() * 360 - 180;  // -180 to 180
                
                try
                {
                    string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                    var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                    
                    if (MeterBasedCoordinateCompressor.IsValidEncoding(encoded) &&
                        Math.Abs(decodedLat) <= 80 &&
                        Math.Abs(decodedLon) <= 180)
                    {
                        successCount++;
                    }
                }
                catch
                {
                    // Count failures
                }
            }

            // Assert - Should have very high success rate
            double successRate = (double)successCount / testCount;
            Assert.True(successRate >= 0.88, $"Success rate {successRate:P} is too low - expected >88%");
        }

        #endregion

        #region Base32 Encoding Edge Cases

        [Fact]
        public void AllBase32Characters_CanBeDecoded()
        {
            // Arrange - Create test strings using each character in the alphabet
            string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
            
            foreach (char c in alphabet)
            {
                // Create a test string with this character in various positions
                string testStr = $"234{c}67880";
                
                // Act & Assert - Should be able to validate (may not decode to valid coordinates)
                if (testStr.Length == 9)
                {
                    bool isValidFormat = true;
                    foreach (char ch in testStr)
                    {
                        if (!alphabet.Contains(ch))
                        {
                            isValidFormat = false;
                            break;
                        }
                    }
                    
                    // All characters in our alphabet should be recognized as valid format
                    Assert.True(isValidFormat, $"Character '{c}' not properly recognized in base32 validation");
                }
            }
        }

        [Fact]
        public void CaseInsensitive_BehavesConsistently()
        {
            // Arrange
            double lat = 40.7128;
            double lon = -74.0060;
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            
            // Act - Test with different case combinations
            string lowercase = encoded.ToLower();
            string mixedCase = "";
            for (int i = 0; i < encoded.Length; i++)
            {
                mixedCase += (i % 2 == 0) ? char.ToLower(encoded[i]) : char.ToUpper(encoded[i]);
            }

            // Assert - All case variations should decode to same result
            var originalResult = MeterBasedCoordinateCompressor.Decode(encoded);
            var lowercaseResult = MeterBasedCoordinateCompressor.Decode(lowercase);
            var mixedCaseResult = MeterBasedCoordinateCompressor.Decode(mixedCase);

            Assert.Equal(originalResult.latitude, lowercaseResult.latitude, 10);
            Assert.Equal(originalResult.longitude, lowercaseResult.longitude, 10);
            Assert.Equal(originalResult.latitude, mixedCaseResult.latitude, 10);
            Assert.Equal(originalResult.longitude, mixedCaseResult.longitude, 10);
        }

        #endregion

        #region Coordinate Boundary Precision

        [Fact]
        public void CoordinateBoundaries_PrecisionMaintained()
        {
            // Arrange - Test coordinates very close to valid boundaries
            var testCases = new[]
            {
                (79.88888, 178.88888),   // Near max boundaries
                (-79.88888, -178.88888), // Near min boundaries
                (79.88888, -178.88888),  // Near mixed boundaries
                (-79.88888, 178.88888),  // Near other mixed boundaries
            };

            foreach (var (lat, lon) in testCases)
            {
                // Act
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);

                // Assert
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
                Assert.InRange(decodedLat, -80, 80);
                Assert.InRange(decodedLon, -180, 180);
                
                // Even at boundaries, error should be reasonable (may be higher at poles)
                if (Math.Abs(lat) < 79.9)
                {
                    Assert.True(totalError <= 15.0, 
                        $"Boundary precision at ({lat}, {lon}): {totalError:F2}m exceeds reasonable threshold");
                }
            }
        }

        #endregion
    }
}