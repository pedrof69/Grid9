package com.grid9.demo;

import com.grid9.core.UniformPrecisionCoordinateCompressor;
import com.grid9.core.CoordinateOperations;

/**
 * Grid9 Demo Application
 * Demonstrates the core functionality of the Grid9 coordinate compression system
 */
public class Grid9Demo {

    public static void main(String[] args) {
        System.out.println("=== Grid9 Coordinate Compression Demo ===\n");
        
        // Demo 1: Basic encoding and decoding
        basicEncodingDemo();
        
        // Demo 2: Human-readable formatting
        formattingDemo();
        
        // Demo 3: Precision measurement
        precisionDemo();
        
        // Demo 4: Distance calculations
        distanceDemo();
        
        // Demo 5: Neighbor finding
        neighborDemo();
        
        // Demo 6: Batch operations
        batchOperationsDemo();
        
        // Demo 7: Nearby search
        nearbySearchDemo();
        
        System.out.println("=== Demo Complete ===");
    }

    private static void basicEncodingDemo() {
        System.out.println("1. Basic Encoding and Decoding");
        System.out.println("===============================");
        
        // Famous world locations
        double[][] locations = {
            {40.7128, -74.0060, 0}, // New York City
            {51.5074, -0.1278, 0},  // London
            {35.6762, 139.6503, 0}, // Tokyo
            {-33.8688, 151.2083, 0}, // Sydney
        };
        String[] names = {"New York City", "London", "Tokyo", "Sydney"};
        
        for (int i = 0; i < locations.length; i++) {
            double lat = locations[i][0];
            double lon = locations[i][1];
            String name = names[i];
            
            String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            double[] decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
            
            System.out.printf("%-15s: (%.4f, %.4f) → %s → (%.6f, %.6f)%n", 
                name, lat, lon, encoded, decoded[0], decoded[1]);
        }
        System.out.println();
    }

    private static void formattingDemo() {
        System.out.println("2. Human-Readable Formatting");
        System.out.println("=============================");
        
        double lat = 40.7128, lon = -74.0060;
        String compact = UniformPrecisionCoordinateCompressor.encode(lat, lon, false);
        String readable = UniformPrecisionCoordinateCompressor.encode(lat, lon, true);
        
        System.out.printf("Compact format:  %s%n", compact);
        System.out.printf("Readable format: %s%n", readable);
        System.out.printf("Is readable?     %s%n", 
            UniformPrecisionCoordinateCompressor.isFormattedForHumans(readable));
        System.out.printf("Unformat:        %s%n", 
            UniformPrecisionCoordinateCompressor.removeFormatting(readable));
        System.out.println();
    }

    private static void precisionDemo() {
        System.out.println("3. Precision Measurement");
        System.out.println("========================");
        
        double[][] testLocations = {
            {0, 0},       // Equator
            {45, 0},      // Mid-latitude
            {70, 0},      // High latitude
            {40.7128, -74.0060}, // NYC
        };
        String[] names = {"Equator", "Mid-latitude", "High latitude", "New York"};
        
        for (int i = 0; i < testLocations.length; i++) {
            double lat = testLocations[i][0];
            double lon = testLocations[i][1];
            String name = names[i];
            
            double[] precision = UniformPrecisionCoordinateCompressor.getActualPrecision(lat, lon);
            System.out.printf("%-13s (%.1f°, %.1f°): X=%.1fm, Y=%.1fm, Total=%.1fm%n", 
                name, lat, lon, precision[0], precision[1], precision[2]);
        }
        System.out.println();
    }

    private static void distanceDemo() {
        System.out.println("4. Distance Calculations");
        System.out.println("========================");
        
        String nyc = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
        String london = UniformPrecisionCoordinateCompressor.encode(51.5074, -0.1278);
        String tokyo = UniformPrecisionCoordinateCompressor.encode(35.6762, 139.6503);
        
        System.out.printf("NYC:    %s%n", nyc);
        System.out.printf("London: %s%n", london);
        System.out.printf("Tokyo:  %s%n", tokyo);
        System.out.println();
        
        double nycToLondon = UniformPrecisionCoordinateCompressor.calculateDistance(nyc, london);
        double nycToTokyo = UniformPrecisionCoordinateCompressor.calculateDistance(nyc, tokyo);
        double londonToTokyo = UniformPrecisionCoordinateCompressor.calculateDistance(london, tokyo);
        
        System.out.printf("NYC to London:     %,.0f meters (%.0f km)%n", nycToLondon, nycToLondon / 1000);
        System.out.printf("NYC to Tokyo:      %,.0f meters (%.0f km)%n", nycToTokyo, nycToTokyo / 1000);
        System.out.printf("London to Tokyo:   %,.0f meters (%.0f km)%n", londonToTokyo, londonToTokyo / 1000);
        System.out.println();
    }

    private static void neighborDemo() {
        System.out.println("5. Neighbor Finding");
        System.out.println("===================");
        
        String center = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
        String[] neighbors = UniformPrecisionCoordinateCompressor.getNeighbors(center);
        
        System.out.printf("Center: %s (NYC)%n", center);
        System.out.printf("Found %d neighbors:%n", neighbors.length);
        for (int i = 0; i < neighbors.length; i++) {
            String neighbor = neighbors[i];
            double distance = UniformPrecisionCoordinateCompressor.calculateDistance(center, neighbor);
            System.out.printf("  %s (%.1fm away)%n", neighbor, distance);
        }
        System.out.println();
    }

    private static void batchOperationsDemo() {
        System.out.println("6. Batch Operations");
        System.out.println("===================");
        
        // Prepare test coordinates
        double[][] coordinates = {
            {40.7128, -74.0060}, // NYC
            {51.5074, -0.1278},  // London
            {35.6762, 139.6503}, // Tokyo
            {-33.8688, 151.2083}, // Sydney
            {55.7558, 37.6176},  // Moscow
        };
        
        long startTime = System.nanoTime();
        String[] encoded = CoordinateOperations.batchEncode(coordinates);
        long encodeTime = System.nanoTime() - startTime;
        
        startTime = System.nanoTime();
        double[][] decoded = CoordinateOperations.batchDecode(encoded);
        long decodeTime = System.nanoTime() - startTime;
        
        System.out.printf("Batch encoded %d coordinates in %.2f μs (%.1f ns each)%n", 
            coordinates.length, encodeTime / 1000.0, encodeTime / (double)coordinates.length);
        System.out.printf("Batch decoded %d coordinates in %.2f μs (%.1f ns each)%n", 
            encoded.length, decodeTime / 1000.0, decodeTime / (double)encoded.length);
        
        System.out.println("Results:");
        String[] names = {"NYC", "London", "Tokyo", "Sydney", "Moscow"};
        for (int i = 0; i < encoded.length; i++) {
            System.out.printf("  %-7s: %s → (%.6f, %.6f)%n", 
                names[i], encoded[i], decoded[i][0], decoded[i][1]);
        }
        System.out.println();
    }

    private static void nearbySearchDemo() {
        System.out.println("7. Nearby Search");
        System.out.println("================");
        
        // Search around Times Square, NYC
        double centerLat = 40.7580, centerLon = -73.9855;
        double radius = 500; // 500 meters
        
        System.out.printf("Searching within %.0fm of Times Square (%.4f, %.4f)%n", 
            radius, centerLat, centerLon);
        
        long startTime = System.nanoTime();
        String[] nearby = CoordinateOperations.findNearby(centerLat, centerLon, radius, 20);
        long searchTime = System.nanoTime() - startTime;
        
        System.out.printf("Found %d locations in %.2f ms:%n", nearby.length, searchTime / 1_000_000.0);
        
        String centerEncoded = UniformPrecisionCoordinateCompressor.encode(centerLat, centerLon);
        for (int i = 0; i < Math.min(10, nearby.length); i++) {
            String location = nearby[i];
            double distance = UniformPrecisionCoordinateCompressor.calculateDistance(centerEncoded, location);
            double[] coords = UniformPrecisionCoordinateCompressor.decode(location);
            System.out.printf("  %s: (%.6f, %.6f) - %.1fm away%n", 
                location, coords[0], coords[1], distance);
        }
        if (nearby.length > 10) {
            System.out.printf("  ... and %d more%n", nearby.length - 10);
        }
        System.out.println();
    }
}