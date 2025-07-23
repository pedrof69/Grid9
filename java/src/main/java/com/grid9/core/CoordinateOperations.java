package com.grid9.core;

import java.util.ArrayList;
import java.util.List;

/**
 * High-performance coordinate operations with batch processing capabilities.
 */
public class CoordinateOperations {

    /**
     * Batch encodes multiple coordinate pairs for high-throughput scenarios.
     * @param coordinates Array of coordinate pairs [lat, lon]
     * @return Array of encoded Grid9 strings
     */
    public static String[] batchEncode(double[][] coordinates) {
        String[] results = new String[coordinates.length];
        
        for (int i = 0; i < coordinates.length; i++) {
            double lat = coordinates[i][0];
            double lon = coordinates[i][1];
            results[i] = UniformPrecisionCoordinateCompressor.encode(lat, lon);
        }
        
        return results;
    }

    /**
     * Batch decodes multiple encoded strings for high-throughput scenarios.
     * @param encoded Array of encoded Grid9 strings
     * @return Array of coordinate pairs [lat, lon]
     */
    public static double[][] batchDecode(String[] encoded) {
        double[][] results = new double[encoded.length][2];
        
        for (int i = 0; i < encoded.length; i++) {
            double[] coords = UniformPrecisionCoordinateCompressor.decode(encoded[i]);
            results[i] = coords;
        }
        
        return results;
    }

    /**
     * Finds all coordinates within a specified radius (in meters) of a center point.
     * Returns encoded strings of nearby coordinates.
     * @param centerLat Center latitude in degrees
     * @param centerLon Center longitude in degrees
     * @param radiusMeters Search radius in meters
     * @param maxResults Maximum number of results to return
     * @return Array of encoded Grid9 strings within the radius
     */
    public static String[] findNearby(double centerLat, double centerLon, double radiusMeters, int maxResults) {
        List<String> results = new ArrayList<>();
        String centerEncoded = UniformPrecisionCoordinateCompressor.encode(centerLat, centerLon);
        
        // Calculate approximate grid search bounds
        double latDelta = radiusMeters / 111320.0; // Rough conversion
        double lonDelta = radiusMeters / (111320.0 * Math.cos(centerLat * Math.PI / 180.0));
        
        double minLat = Math.max(-80, centerLat - latDelta);
        double maxLat = Math.min(80, centerLat + latDelta);
        double minLon = Math.max(-180, centerLon - lonDelta);
        double maxLon = Math.min(180, centerLon + lonDelta);
        
        // Grid search with 3-meter steps
        final double latStep = 3.0 / 111320.0;
        final double lonStep = 3.0 / 111320.0;
        
        for (double lat = minLat; lat <= maxLat && results.size() < maxResults; lat += latStep) {
            for (double lon = minLon; lon <= maxLon && results.size() < maxResults; lon += lonStep) {
                try {
                    String encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                    double distance = UniformPrecisionCoordinateCompressor.calculateDistance(centerEncoded, encoded);
                    
                    if (distance <= radiusMeters) {
                        results.add(encoded);
                    }
                } catch (IllegalArgumentException e) {
                    // Skip invalid coordinates (out of bounds, etc.)
                    continue;
                }
            }
        }
        
        return results.toArray(new String[0]);
    }

    /**
     * Finds all coordinates within a specified radius (in meters) of a center point.
     * Uses default maximum of 100 results.
     * @param centerLat Center latitude in degrees
     * @param centerLon Center longitude in degrees
     * @param radiusMeters Search radius in meters
     * @return Array of encoded Grid9 strings within the radius
     */
    public static String[] findNearby(double centerLat, double centerLon, double radiusMeters) {
        return findNearby(centerLat, centerLon, radiusMeters, 100);
    }
}