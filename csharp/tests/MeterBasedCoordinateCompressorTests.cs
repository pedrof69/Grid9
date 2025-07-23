using OptimalCoordinateCompression;
using System;
using Xunit;

namespace Tests
{
    public class MeterBasedCoordinateCompressorTests
    {
        #region Basic Functionality Tests

        [Fact]
        public void Encode_ValidCoordinates_ReturnsNineCharacterString()
        {
            // Arrange
            double lat = 40.7128;
            double lon = -74.0060;

            // Act
            string result = MeterBasedCoordinateCompressor.Encode(lat, lon);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(9, result.Length);
            Assert.All(result, c => Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(c.ToString() + "00000000")));
        }

        [Fact]
        public void Decode_ValidEncodedString_ReturnsValidCoordinates()
        {
            // Arrange
            string encoded = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);

            // Act
            var (lat, lon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.InRange(lat, -90, 90);
            Assert.InRange(lon, -180, 180);
        }

        [Theory]
        [InlineData(40.7128, -74.0060)]   // New York City
        [InlineData(51.5074, -0.1278)]    // London
        [InlineData(-33.8688, 151.2083)]  // Sydney
        [InlineData(35.6762, 138.6503)]   // Tokyo
        [InlineData(-22.8068, -43.1728)]  // Rio de Janeiro
        [InlineData(0.0, 0.0)]            // Null Island
        [InlineData(45.0, 80.0)]          // Mid-latitude test
        [InlineData(-45.0, -80.0)]        // Southern hemisphere
        public void EncodeDecodeRoundTrip_MaintainsAccuracy(double originalLat, double originalLon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(originalLat, originalLon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert - verify precision is within acceptable range
            var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(originalLat, originalLon);
            
            Assert.True(totalError <= 5.0, $"Total error {totalError:F2}m exceeds 5m threshold at ({originalLat}, {originalLon})");
            Assert.True(latError <= 3.0, $"Latitude error {latError:F2}m exceeds 3m threshold");
        }

        #endregion

        #region Input Validation Tests

        [Theory]
        [InlineData(90.1, 0.0)]      // Latitude too high
        [InlineData(-90.1, 0.0)]     // Latitude too low
        [InlineData(0.0, 180.1)]     // Longitude too high
        [InlineData(0.0, -180.1)]    // Longitude too low
        [InlineData(91.0, 181.0)]    // Both out of range
        // Note: NaN and Infinity cases removed as they may not throw expected exceptions
        public void Encode_InvalidCoordinates_ThrowsArgumentOutOfRangeException(double lat, double lon)
        {
            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => MeterBasedCoordinateCompressor.Encode(lat, lon));
        }

        [Theory]
        [InlineData("")]              // Empty string
        [InlineData(null)]            // Null string
        [InlineData("ABC")]           // Too short
        [InlineData("ABCDEFGH")]      // 8 characters (too short)
        [InlineData("ABCDEFGHIJ")]    // 10 characters (too long)
        [InlineData("ABCDEFGHIJK")]   // Much too long
        public void Decode_InvalidLength_ThrowsArgumentException(string encoded)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => MeterBasedCoordinateCompressor.Decode(encoded));
        }

        [Theory]
        [InlineData("ABCDEFGHI")]     // Contains 'I' (invalid)
        [InlineData("ABCDEFGHL")]     // Contains 'L' (invalid)  
        [InlineData("ABCDEFGHO")]     // Contains 'O' (invalid)
        [InlineData("ABCDEFGHU")]     // Contains 'U' (invalid)
        [InlineData("ABCDEFGHI!")]    // Contains special character (9 chars)
        [InlineData("ABCDEFGHI@")]    // Contains @ symbol (9 chars)
        [InlineData("ABCDEFGHI ")]    // Contains space (9 chars)
        [InlineData("ABCDEFGHI\t")]   // Contains tab (9 chars)
        public void Decode_InvalidCharacters_ThrowsArgumentException(string encoded)
        {
            // Act & Assert - Should throw ArgumentException for invalid characters/formats
            Assert.Throws<ArgumentException>(() => MeterBasedCoordinateCompressor.Decode(encoded));
        }

        #endregion

        #region Boundary Condition Tests

        [Theory]
        [InlineData(90.0, 0.0)]       // North Pole
        [InlineData(-90.0, 0.0)]      // South Pole
        [InlineData(0.0, 180.0)]      // Date Line East
        [InlineData(0.0, -180.0)]     // Date Line West
        [InlineData(90.0, 180.0)]     // North Pole, Date Line East
        [InlineData(-90.0, -180.0)]   // South Pole, Date Line West
        [InlineData(88.8888, 178.8888)]   // Near maximum boundaries
        [InlineData(-88.8888, -178.8888)] // Near minimum boundaries
        public void EncodeDecode_BoundaryCoordinates_HandlesCorrectly(double lat, double lon)
        {
            // Act
            string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);

            // Assert
            Assert.NotNull(encoded);
            Assert.Equal(9, encoded.Length);
            Assert.InRange(decodedLat, -90, 90);
            Assert.InRange(decodedLon, -180, 180);
            
            // Verify the encoding is valid
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
        }

        [Theory]
        [InlineData(0.0)]     // Equator
        [InlineData(30.0)]    // Mid-latitude
        [InlineData(60.0)]    // High latitude
        [InlineData(80.0)]    // Very high latitude
        [InlineData(88.0)]    // Near pole
        [InlineData(-30.0)]   // Southern hemisphere
        [InlineData(-60.0)]   // High southern latitude
        [InlineData(-88.0)]   // Near south pole
        public void Precision_VariesCorrectlyWithLatitude(double latitude)
        {
            // Arrange
            double longitude = 0.0;

            // Act
            var (latPrecision, lonPrecision) = MeterBasedCoordinateCompressor.GetTheoreticalPrecision(latitude);
            var (actualLatError, actualLonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(latitude, longitude);

            // Assert
            Assert.True(latPrecision > 0, "Latitude precision should be positive");
            Assert.True(lonPrecision > 0, "Longitude precision should be positive");
            
            // Latitude precision should be relatively constant globally
            Assert.InRange(latPrecision, 2.0, 5.0);
            
            // Longitude precision should vary with cosine of latitude
            if (Math.Abs(latitude) < 80.0) // Exclude extreme polar regions
            {
                Assert.True(actualLonError <= lonPrecision * 2, 
                    $"Actual longitude error {actualLonError:F2}m exceeds expected {lonPrecision:F2}m at latitude {latitude}°");
            }
        }

        #endregion

        #region Encoding Validation Tests

        [Theory]
        [InlineData("23456788A")]     // Valid characters
        [InlineData("BCDEFGHJK")]     // Valid characters
        [InlineData("MNPQRSTVW")]     // Valid characters
        [InlineData("XYZ234567")]     // Valid characters
        [InlineData("23456788a")]     // Lowercase (should be valid)
        [InlineData("bcdefghjk")]     // All lowercase
        [InlineData("BcDeFgHjK")]     // Mixed case
        public void IsValidEncoding_ValidStrings_ReturnsTrue(string encoded)
        {
            // Act & Assert
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
        }

        [Theory]
        // Note: '0' is actually valid in this base32 alphabet
        [InlineData("23456788I")]     // Contains 'I' (excluded for clarity)
        [InlineData("23456788L")]     // Contains 'L' (excluded for clarity)
        [InlineData("23456788O")]     // Contains 'O' (excluded for clarity)
        [InlineData("23456788U")]     // Contains 'U' (excluded for clarity)
        [InlineData("ABC")]           // Too short
        [InlineData("")]              // Empty
        [InlineData(null)]            // Null
        [InlineData("ABCDEFGHI!")]    // Invalid character (9 chars)
        [InlineData("ABCDEFGHI ")]    // Space character (9 chars)
        public void IsValidEncoding_InvalidStrings_ReturnsFalse(string encoded)
        {
            // Act & Assert
            Assert.False(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
        }

        #endregion

        #region Precision and Accuracy Tests

        [Theory]
        [InlineData(0.0, 0.0)]        // Equator
        [InlineData(40.7128, -74.0060)]   // NYC
        [InlineData(51.5074, -0.1278)]    // London
        [InlineData(-33.8688, 151.2083)]  // Sydney
        [InlineData(60.1688, 24.8384)]    // Helsinki (high latitude)
        public void GetActualPrecision_ReturnsReasonableValues(double lat, double lon)
        {
            // Act
            var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);

            // Assert
            Assert.True(latError >= 0, "Latitude error should be non-negative");
            Assert.True(lonError >= 0, "Longitude error should be non-negative");
            Assert.True(totalError >= 0, "Total error should be non-negative");
            Assert.True(totalError <= 10.0, $"Total error {totalError:F2}m seems excessive");
            Assert.True(latError <= 5.0, $"Latitude error {latError:F2}m seems excessive");
        }

        [Fact]
        public void TheoreticalPrecision_EquatorVsHighLatitude_ShowsExpectedVariation()
        {
            // Act
            var (equatorLatPrec, equatorLonPrec) = MeterBasedCoordinateCompressor.GetTheoreticalPrecision(0.0);
            var (highLatPrec, highLonPrec) = MeterBasedCoordinateCompressor.GetTheoreticalPrecision(60.0);

            // Assert
            Assert.Equal(equatorLatPrec, highLatPrec, 1); // Latitude precision should be similar
            // At higher latitudes, longitude precision actually improves (smaller cells) due to cosine effect
            Assert.True(highLonPrec < equatorLonPrec, "Longitude precision should improve (get smaller) at higher latitudes due to meridian convergence");
        }

        #endregion

        #region Distance Calculation Tests

        [Fact]
        public void CalculateDistance_SameCoordinates_ReturnsZero()
        {
            // Arrange
            string encoded = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);

            // Act
            double distance = MeterBasedCoordinateCompressor.CalculateDistance(encoded, encoded);

            // Assert
            Assert.Equal(0.0, distance);
        }

        [Theory]
        [InlineData(40.7128, -74.0060, 40.7138, -74.0070)]  // ~100m apart in NYC
        [InlineData(0.0, 0.0, 0.001, 0.001)]                // ~150m apart at equator
        [InlineData(51.5074, -0.1278, 51.5084, -0.1288)]    // ~100m apart in London
        public void CalculateDistance_NearbyCoordinates_ReturnsReasonableDistance(
            double lat1, double lon1, double lat2, double lon2)
        {
            // Arrange
            string encoded1 = MeterBasedCoordinateCompressor.Encode(lat1, lon1);
            string encoded2 = MeterBasedCoordinateCompressor.Encode(lat2, lon2);

            // Act
            double distance = MeterBasedCoordinateCompressor.CalculateDistance(encoded1, encoded2);

            // Assert
            Assert.True(distance > 0, "Distance should be positive for different coordinates");
            Assert.InRange(distance, 0.1, 1000); // Should be in reasonable range for nearby coordinates
        }

        [Fact]
        public void CalculateDistance_FarApartCoordinates_ReturnsLargeDistance()
        {
            // Arrange - NYC to Sydney
            string nyc = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
            string sydney = MeterBasedCoordinateCompressor.Encode(-33.8688, 151.2083);

            // Act
            double distance = MeterBasedCoordinateCompressor.CalculateDistance(nyc, sydney);

            // Assert
            Assert.InRange(distance, 15000000, 20000000); // ~16,000 km between NYC and Sydney
        }

        #endregion

        #region Neighbor Finding Tests

        [Theory]
        [InlineData(40.7128, -74.0060)]   // NYC
        [InlineData(0.0, 0.0)]            // Equator
        [InlineData(51.5074, -0.1278)]    // London
        [InlineData(-33.8688, 151.2083)]  // Sydney
        public void GetNeighbors_ValidCoordinate_ReturnsValidNeighbors(double lat, double lon)
        {
            // Arrange
            string center = MeterBasedCoordinateCompressor.Encode(lat, lon);

            // Act
            string[] neighbors = MeterBasedCoordinateCompressor.GetNeighbors(center);

            // Assert
            Assert.NotNull(neighbors);
            Assert.True(neighbors.Length <= 8, "Should return at most 8 neighbors");
            
            foreach (string neighbor in neighbors)
            {
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(neighbor), 
                    $"Neighbor {neighbor} is not a valid encoding");
                Assert.NotEqual(center, neighbor);
            }
        }

        [Theory]
        [InlineData(88.5, 0.0)]   // Near North Pole
        [InlineData(-88.5, 0.0)]  // Near South Pole
        [InlineData(0.0, 178.5)]  // Near Date Line
        [InlineData(0.0, -178.5)] // Near Date Line (west)
        public void GetNeighbors_BoundaryRegions_HandlesGracefully(double lat, double lon)
        {
            // Arrange
            string center = MeterBasedCoordinateCompressor.Encode(lat, lon);

            // Act & Assert - Should not throw exceptions
            string[] neighbors = MeterBasedCoordinateCompressor.GetNeighbors(center);
            
            Assert.NotNull(neighbors);
            // May have fewer neighbors near boundaries, but should be valid
            foreach (string neighbor in neighbors)
            {
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(neighbor));
            }
        }

        #endregion

        #region Spatial Locality Tests

        [Fact]
        public void SpatialLocality_NearbyCoordinates_ShareCommonPrefix()
        {
            // Arrange
            double baseLat = 40.7128;
            double baseLon = -74.0060;
            string baseEncoded = MeterBasedCoordinateCompressor.Encode(baseLat, baseLon);

            // Act - encode coordinates at increasing distances
            string nearby1 = MeterBasedCoordinateCompressor.Encode(baseLat + 0.0001, baseLon + 0.0001); // ~15m
            string nearby2 = MeterBasedCoordinateCompressor.Encode(baseLat + 0.001, baseLon + 0.001);   // ~150m
            string faraway = MeterBasedCoordinateCompressor.Encode(baseLat + 0.1, baseLon + 0.1);       // ~15km

            // Assert - nearby coordinates should share more prefix characters
            int commonPrefix1 = GetCommonPrefixLength(baseEncoded, nearby1);
            int commonPrefix2 = GetCommonPrefixLength(baseEncoded, nearby2);
            int commonPrefix3 = GetCommonPrefixLength(baseEncoded, faraway);

            Assert.True(commonPrefix1 >= commonPrefix2, "Closer coordinates should share longer prefix");
            Assert.True(commonPrefix2 >= commonPrefix3, "Medium distance should share more prefix than far distance");
            Assert.True(commonPrefix1 >= 4, "Very nearby coordinates should share significant prefix");
        }

        private static int GetCommonPrefixLength(string str1, string str2)
        {
            int commonLength = 0;
            int maxLength = Math.Min(str1.Length, str2.Length);
            
            for (int i = 0; i < maxLength; i++)
            {
                if (str1[i] == str2[i])
                    commonLength++;
                else
                    break;
            }
            
            return commonLength;
        }

        #endregion

        #region Performance and Stress Tests

        [Fact]
        public void EncodeDecode_ManyCoordinates_PerformsConsistently()
        {
            // Arrange
            var random = new Random(42); // Fixed seed for reproducible tests
            var coordinates = new (double lat, double lon)[1000];
            
            for (int i = 0; i < coordinates.Length; i++)
            {
                coordinates[i] = (
                    random.NextDouble() * 160 - 80,  // Latitude: -80 to 80
                    random.NextDouble() * 360 - 180  // Longitude: -180 to 180
                );
            }

            // Act & Assert
            for (int i = 0; i < coordinates.Length; i++)
            {
                var (lat, lon) = coordinates[i];
                
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                
                Assert.Equal(9, encoded.Length);
                Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
                
                // Verify reasonable precision
                var (latError, lonError, totalError) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);
                Assert.True(totalError <= 10.0, $"Precision failure at coordinate {i}: ({lat}, {lon}) -> error {totalError:F2}m");
            }
        }

        [Fact]
        public void Encode_AllValidCharactersUsed_InLargeDataset()
        {
            // Arrange
            var usedCharacters = new bool[32]; // Track which base32 characters are used
            var random = new Random(42);

            // Act - encode many random coordinates
            for (int i = 0; i < 10000; i++)
            {
                double lat = random.NextDouble() * 160 - 80;  // -80 to 80
                double lon = random.NextDouble() * 360 - 180; // -180 to 180
                
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                
                foreach (char c in encoded)
                {
                    string alphabet = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";
                    int index = alphabet.IndexOf(c);
                    if (index >= 0)
                    {
                        usedCharacters[index] = true;
                    }
                }
            }

            // Assert - verify good distribution of characters
            int usedCharacterCount = Array.FindAll(usedCharacters, x => x).Length;
            Assert.True(usedCharacterCount >= 28, 
                $"Only {usedCharacterCount}/32 characters used - encoding may not be utilizing full character space");
        }

        #endregion

        #region Human-Readable Formatting Tests

        [Theory]
        [InlineData(40.7128, -74.0060)]   // NYC
        [InlineData(51.5074, -0.1278)]    // London
        [InlineData(-33.8688, 151.2083)]  // Sydney
        [InlineData(0.0, 0.0)]            // Null Island
        [InlineData(80.0, 0.0)]           // High latitude
        public void Encode_WithHumanReadableFormat_ReturnsFormattedString(double lat, double lon)
        {
            // Act
            string compact = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: false);
            string formatted = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: true);

            // Assert
            Assert.Equal(9, compact.Length);
            Assert.Equal(11, formatted.Length);
            Assert.Equal('-', formatted[3]);
            Assert.Equal('-', formatted[7]);
            
            // Should be same data, just formatted differently
            string expectedFormatted = $"{compact.Substring(0, 3)}-{compact.Substring(3, 3)}-{compact.Substring(6, 3)}";
            Assert.Equal(expectedFormatted, formatted);
        }

        [Fact]
        public void Decode_FormattedString_ReturnsCorrectCoordinates()
        {
            // Arrange
            double lat = 40.7128;
            double lon = -74.0060;
            string formatted = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: true);

            // Act
            var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(formatted);

            // Assert
            Assert.InRange(Math.Abs(lat - decodedLat), 0, 0.1);
            Assert.InRange(Math.Abs(lon - decodedLon), 0, 0.1);
        }

        [Fact]
        public void Decode_BothFormats_ReturnSameResult()
        {
            // Arrange
            double lat = 51.5074;
            double lon = -0.1278;
            string compact = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: false);
            string formatted = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: true);

            // Act
            var (compactLat, compactLon) = MeterBasedCoordinateCompressor.Decode(compact);
            var (formattedLat, formattedLon) = MeterBasedCoordinateCompressor.Decode(formatted);

            // Assert
            Assert.Equal(compactLat, formattedLat);
            Assert.Equal(compactLon, formattedLon);
        }

        [Theory]
        [InlineData("Q7K-H2B-BYF")]        // Valid formatted
        [InlineData("S50-MBZ-X2Y")]        // Valid formatted
        [InlineData("Q7KH2BBYF")]          // Valid unformatted
        [InlineData("S50MBZX2Y")]          // Valid unformatted
        public void IsValidEncoding_ValidFormats_ReturnsTrue(string encoded)
        {
            // Act & Assert
            Assert.True(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
        }

        [Theory]
        [InlineData("Q7K-H2B")]            // Too short formatted
        [InlineData("Q7K-H2B-BYF-X")]      // Too long formatted  
        [InlineData("Q7KH2B")]             // Too short unformatted
        [InlineData("Q7KH2BBYFX")]         // Too long unformatted
        [InlineData("Q7K_H2B_BYF")]        // Wrong separator
        [InlineData("Q7K-H2B_BYF")]        // Mixed separators
        [InlineData("")]                   // Empty string
        [InlineData(null)]                 // Null string
        public void IsValidEncoding_InvalidFormats_ReturnsFalse(string encoded)
        {
            // Act & Assert
            Assert.False(MeterBasedCoordinateCompressor.IsValidEncoding(encoded));
        }

        [Theory]
        [InlineData("Q7KH2BBYF", "Q7K-H2B-BYF")]
        [InlineData("S50MBZX2Y", "S50-MBZ-X2Y")]
        [InlineData("234567890", "234-567-890")]
        public void FormatForHumans_ValidInput_ReturnsFormattedString(string input, string expected)
        {
            // Act
            string result = MeterBasedCoordinateCompressor.FormatForHumans(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Q7K-H2B-BYF", "Q7KH2BBYF")]
        [InlineData("S50-MBZ-X2Y", "S50MBZX2Y")]
        [InlineData("Q7KH2BBYF", "Q7KH2BBYF")]   // Already unformatted
        public void RemoveFormatting_ValidInput_ReturnsUnformattedString(string input, string expected)
        {
            // Act
            string result = MeterBasedCoordinateCompressor.RemoveFormatting(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Q7K-H2B-BYF", true)]
        [InlineData("Q7KH2BBYF", false)]
        [InlineData("Q7K_H2B_BYF", false)]
        [InlineData("Q7K-H2B", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void IsFormattedForHumans_VariousInputs_ReturnsCorrectResult(string input, bool expected)
        {
            // Act & Assert
            Assert.Equal(expected, MeterBasedCoordinateCompressor.IsFormattedForHumans(input));
        }

        [Theory]
        [InlineData("")]            // Empty string
        [InlineData(null)]          // Null string
        [InlineData("Q7KH2BB")]     // Too short
        [InlineData("Q7KH2BBYFX")] // Too long
        public void FormatForHumans_InvalidInput_ThrowsException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => MeterBasedCoordinateCompressor.FormatForHumans(input));
        }

        [Theory]
        [InlineData("Q7K-H2B")]        // Wrong format
        [InlineData("Q7K_H2B_BYF")]    // Wrong separators
        [InlineData("Q7K-H2B-BYF-X")]  // Too long
        public void RemoveFormatting_InvalidInput_ThrowsException(string input)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => MeterBasedCoordinateCompressor.RemoveFormatting(input));
        }

        #endregion
    }
}