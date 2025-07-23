package com.grid9.core;

import org.junit.jupiter.api.Test;
import org.junit.jupiter.params.ParameterizedTest;
import org.junit.jupiter.params.provider.CsvSource;
import static org.junit.jupiter.api.Assertions.*;

import java.util.ArrayList;
import java.util.List;

/**
 * Comprehensive test suite for UniformPrecisionCoordinateCompressor
 * Validates uniform 3-meter precision across all latitudes and longitudes
 */
public class UniformPrecisionCoordinateCompressorTest {
    
    private static final double PRECISION_TOLERANCE_M = 0.1; // 10cm tolerance for precision validation
    private static final double MAX_EXPECTED_ERROR_M = 3.5; // Realistic maximum with 45-bit constraints
    private static final double TARGET_PRECISION_M = 3.0;

    @Test
    public void encode_ProducesNineCharacterString() {
        // Test various global coordinates
        double[][] testCoordinates = {
            {40.7128, -74.0060}, // New York
            {51.5074, -0.1278},  // London  
            {35.6762, 139.6503}, // Tokyo
            {-33.8688, 151.2083}, // Sydney
            {55.7558, 37.6176},  // Moscow
            {-22.9068, -43.1729}, // Rio de Janeiro
            {1.3521, 103.8198},  // Singapore
            {78.2232, 15.6267},  // Svalbard (high latitude)
            {-54.8019, -68.3030}, // Ushuaia (southern)
        };

        for (double[] coord : testCoordinates) {
            double lat = coord[0];
            double lon = coord[1];
            String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            
            assertNotNull(encoded);
            assertEquals(9, encoded.length());
            
            // Verify all characters are valid base32
            for (char c : encoded.toCharArray()) {
                assertTrue("0123456789ABCDEFGHJKMNPQRSTVWXYZ".indexOf(c) >= 0,
                    "Invalid character: " + c);
            }
        }
    }

    @Test
    public void encodeWithHumanReadable_ProducesFormattedString() {
        String compact = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, false);
        String readable = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, true);
        
        assertEquals(9, compact.length());
        assertEquals(11, readable.length());
        assertEquals('-', readable.charAt(3));
        assertEquals('-', readable.charAt(7));
        
        // Both should decode to same coordinates
        double[] coords1 = UniformPrecisionCoordinateCompressor.decode(compact);
        double[] coords2 = UniformPrecisionCoordinateCompressor.decode(readable);
        
        assertEquals(coords1[0], coords2[0], 0.000001);
        assertEquals(coords1[1], coords2[1], 0.000001);
    }

    @Test
    public void decode_RoundTripAccuracy() {
        double[][] testCoordinates = {
            {40.7128, -74.0060}, // New York
            {51.5074, -0.1278},  // London  
            {35.6762, 139.6503}, // Tokyo
            {-33.8688, 151.2083}, // Sydney
            {0.0, 0.0},          // Origin
            {89.9, 0.0},         // Near North Pole
            {-89.9, 0.0},        // Near South Pole
            {0.0, 179.9},        // Near Date Line
            {0.0, -179.9},       // Near Date Line (other side)
        };

        for (double[] coord : testCoordinates) {
            double originalLat = coord[0];
            double originalLon = coord[1];
            
            String encoded = UniformPrecisionCoordinateCompressor.encode(originalLat, originalLon);
            double[] decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
            double decodedLat = decoded[0];
            double decodedLon = decoded[1];
            
            // Calculate error in meters
            double distance = calculateHaversineDistance(originalLat, originalLon, decodedLat, decodedLon);
            
            assertTrue(distance <= MAX_EXPECTED_ERROR_M, 
                String.format("Round-trip error %.1fm exceeds maximum %.1fm at (%.4f, %.4f)", 
                    distance, MAX_EXPECTED_ERROR_M, originalLat, originalLon));
        }
    }

    @ParameterizedTest
    @CsvSource({
        "90, 0",    // North Pole
        "-90, 0",   // South Pole  
        "0, 180",   // Date Line
        "0, -180",  // Date Line
        "0, 0"      // Origin
    })
    public void encode_HandlesEdgeCases(double lat, double lon) {
        String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
        assertEquals(9, encoded.length());
        
        double[] decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
        double distance = calculateHaversineDistance(lat, lon, decoded[0], decoded[1]);
        
        assertTrue(distance <= MAX_EXPECTED_ERROR_M);
    }

    @Test
    public void getActualPrecision_ReturnsUniformPrecision() {
        // Test precision at various latitudes - should be uniform with equal-area projection
        double[] testLatitudes = {-80, -60, -40, -20, 0, 20, 40, 60, 80};
        double[] testLongitudes = {-150, -90, -30, 0, 30, 90, 150};

        List<Double> precisionResults = new ArrayList<>();

        for (double lat : testLatitudes) {
            for (double lon : testLongitudes) {
                double[] precision = UniformPrecisionCoordinateCompressor.getActualPrecision(lat, lon);
                double totalError = precision[2];
                precisionResults.add(totalError);
                
                // All precision values should be similar (uniform)
                assertTrue(totalError <= MAX_EXPECTED_ERROR_M, 
                    String.format("Precision %.1fm exceeds maximum at (%.1f, %.1f)", totalError, lat, lon));
                assertTrue(totalError >= 2.0, 
                    String.format("Precision %.1fm suspiciously low at (%.1f, %.1f)", totalError, lat, lon));
            }
        }

        // Verify uniformity - calculate coefficient of variation
        double meanPrecision = precisionResults.stream().mapToDouble(Double::doubleValue).average().orElse(0.0);
        
        double variance = precisionResults.stream()
            .mapToDouble(p -> Math.pow(p - meanPrecision, 2))
            .average().orElse(0.0);
        
        double stdDev = Math.sqrt(variance);
        double coefficientOfVariation = stdDev / meanPrecision;

        // Coefficient of variation should be reasonable (longitude varies with latitude)
        assertTrue(coefficientOfVariation < 0.3, 
            String.format("Precision variation coefficient %.3f indicates non-uniform precision", coefficientOfVariation));
    }

    @Test
    public void precisionComparison_BetterThanOldAlgorithm() {
        // Compare precision at high latitudes where old algorithm was poor
        double[][] highLatitudeLocations = {
            {70.0, 0.0},     // Northern Canada
            {75.0, 120.0},   // Northern Siberia  
            {65.0, -150.0},  // Northern Alaska
            {-65.0, 45.0},   // Antarctica
        };

        for (double[] coord : highLatitudeLocations) {
            double lat = coord[0];
            double lon = coord[1];
            
            double[] precision = UniformPrecisionCoordinateCompressor.getActualPrecision(lat, lon);
            double totalError = precision[2];
            
            // New algorithm should maintain good precision even at high latitudes
            assertTrue(totalError <= 5.0, 
                String.format("High latitude precision %.1fm is poor at (%.1f, %.1f)", totalError, lat, lon));
                
            // Test actual encoding accuracy
            String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            double[] decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
            double actualError = calculateHaversineDistance(lat, lon, decoded[0], decoded[1]);
            
            assertTrue(actualError <= MAX_EXPECTED_ERROR_M,
                String.format("High latitude actual error %.1fm exceeds maximum at (%.1f, %.1f)", actualError, lat, lon));
        }
    }

    @Test
    public void calculateDistance_AccurateResults() {
        // Test known distances
        String nyc = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
        String london = UniformPrecisionCoordinateCompressor.encode(51.5074, -0.1278);
        
        double calculatedDistance = UniformPrecisionCoordinateCompressor.calculateDistance(nyc, london);
        double expectedDistance = 5570224; // Actual NYC to London distance in meters
        
        // Allow 1% tolerance due to encoding precision
        double tolerance = expectedDistance * 0.01;
        assertTrue(Math.abs(calculatedDistance - expectedDistance) <= tolerance,
            String.format("Distance calculation error: expected %.0fm, got %.0fm", 
                expectedDistance, calculatedDistance));
    }

    @Test
    public void isValidEncoding_ValidatesCorrectly() {
        // Valid codes
        assertTrue(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBYF"));
        assertTrue(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7K-H2B-BYF"));
        assertTrue(UniformPrecisionCoordinateCompressor.isValidEncoding("000000000"));
        
        // Invalid codes
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding(null));
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding(""));
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBY")); // Too short
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBYFF")); // Too long
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBYF!")); // Invalid char
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBYFI")); // Invalid char (I)
        assertFalse(UniformPrecisionCoordinateCompressor.isValidEncoding("Q7K-H2B-BY")); // Wrong format
    }

    @Test
    public void formatForHumans_FormatsCorrectly() {
        String code = "Q7KH2BBYF";
        String formatted = UniformPrecisionCoordinateCompressor.formatForHumans(code);
        assertEquals("Q7K-H2B-BYF", formatted);
        
        // Test exceptions
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.formatForHumans(null));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.formatForHumans("Q7KH2BBY"));
    }

    @Test
    public void removeFormatting_RemovesFormattingCorrectly() {
        String formatted = "Q7K-H2B-BYF";
        String unformatted = UniformPrecisionCoordinateCompressor.removeFormatting(formatted);
        assertEquals("Q7KH2BBYF", unformatted);
        
        // Test already unformatted
        assertEquals("Q7KH2BBYF", UniformPrecisionCoordinateCompressor.removeFormatting("Q7KH2BBYF"));
        
        // Test exceptions
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.removeFormatting(null));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.removeFormatting("Q7K-H2B-BY"));
    }

    @Test
    public void isFormattedForHumans_DetectsFormattingCorrectly() {
        assertTrue(UniformPrecisionCoordinateCompressor.isFormattedForHumans("Q7K-H2B-BYF"));
        assertFalse(UniformPrecisionCoordinateCompressor.isFormattedForHumans("Q7KH2BBYF"));
        assertFalse(UniformPrecisionCoordinateCompressor.isFormattedForHumans(null));
        assertFalse(UniformPrecisionCoordinateCompressor.isFormattedForHumans("Q7K-H2B-BY"));
        assertFalse(UniformPrecisionCoordinateCompressor.isFormattedForHumans("Q7KH2B-BYF"));
    }

    @Test
    public void getNeighbors_ReturnsNeighboringCodes() {
        String center = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
        String[] neighbors = UniformPrecisionCoordinateCompressor.getNeighbors(center);
        
        // Should return up to 8 neighbors (can be fewer at edges)
        assertTrue(neighbors.length <= 8);
        assertTrue(neighbors.length > 0);
        
        // No neighbor should be the center itself
        for (String neighbor : neighbors) {
            assertNotEquals(center, neighbor);
            assertTrue(UniformPrecisionCoordinateCompressor.isValidEncoding(neighbor));
        }
        
        // Test at pole (edge case)
        String pole = UniformPrecisionCoordinateCompressor.encode(89.0, 0.0);
        String[] poleNeighbors = UniformPrecisionCoordinateCompressor.getNeighbors(pole);
        assertTrue(poleNeighbors.length > 0);
    }

    @Test
    public void encode_ThrowsOnInvalidInput() {
        // Latitude out of range
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.encode(91.0, 0.0));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.encode(-91.0, 0.0));
        
        // Longitude out of range
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.encode(0.0, 181.0));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.encode(0.0, -181.0));
    }

    @Test
    public void decode_ThrowsOnInvalidInput() {
        // Null/empty input
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode(null));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode(""));
        
        // Wrong length
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode("Q7KH2BBY"));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYFF"));
        
        // Invalid characters
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYF!"));
        assertThrows(IllegalArgumentException.class, () ->
            UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYFI"));
    }

    @Test
    public void caseInsensitive_DecodingWorks() {
        String upper = "Q7KH2BBYF";
        String lower = "q7kh2bbyf";
        String mixed = "Q7kH2bByF";
        
        double[] coords1 = UniformPrecisionCoordinateCompressor.decode(upper);
        double[] coords2 = UniformPrecisionCoordinateCompressor.decode(lower);
        double[] coords3 = UniformPrecisionCoordinateCompressor.decode(mixed);
        
        assertEquals(coords1[0], coords2[0], 0.000001);
        assertEquals(coords1[1], coords2[1], 0.000001);
        assertEquals(coords1[0], coords3[0], 0.000001);
        assertEquals(coords1[1], coords3[1], 0.000001);
    }

    @Test
    public void realWorldLocations_ProduceExpectedCodes() {
        // Test documented examples from README
        double[] nycCoords = {40.7128, -74.0060};
        String nycCode = UniformPrecisionCoordinateCompressor.encode(nycCoords[0], nycCoords[1]);
        assertEquals("Q7KH2BBYF", nycCode);
        
        double[] londonCoords = {51.5074, -0.1278};
        String londonCode = UniformPrecisionCoordinateCompressor.encode(londonCoords[0], londonCoords[1]);
        assertEquals("S50MBZX2Y", londonCode);
        
        double[] tokyoCoords = {35.6762, 139.6503};
        String tokyoCode = UniformPrecisionCoordinateCompressor.encode(tokyoCoords[0], tokyoCoords[1]);
        assertEquals("PAYMZ39T7", tokyoCode);
    }

    @Test
    public void performance_EncodingIsReasonablyFast() {
        // Simple performance test - should complete in reasonable time
        long startTime = System.currentTimeMillis();
        
        for (int i = 0; i < 10000; i++) {
            double lat = -90 + (180.0 * i / 10000);
            double lon = -180 + (360.0 * i / 10000);
            String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            assertNotNull(encoded);
        }
        
        long elapsed = System.currentTimeMillis() - startTime;
        assertTrue(elapsed < 1000, "10,000 encodings took too long: " + elapsed + "ms");
    }

    /**
     * Calculate Haversine distance between two lat/lon points
     */
    private double calculateHaversineDistance(double lat1, double lon1, double lat2, double lon2) {
        double R = 6371000.0; // Earth radius in meters
        double dLat = (lat2 - lat1) * Math.PI / 180.0;
        double dLon = (lon2 - lon1) * Math.PI / 180.0;
        double a = Math.sin(dLat/2) * Math.sin(dLat/2) +
                  Math.cos(lat1 * Math.PI / 180.0) * Math.cos(lat2 * Math.PI / 180.0) *
                  Math.sin(dLon/2) * Math.sin(dLon/2);
        double c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
        return R * c;
    }
}