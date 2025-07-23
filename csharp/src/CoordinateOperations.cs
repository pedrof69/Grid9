using System;
using System.Collections.Generic;

namespace OptimalCoordinateCompression
{
    /// <summary>
    /// High-performance coordinate operations with batch processing capabilities.
    /// </summary>
    public static class CoordinateOperations
    {
        /// <summary>
        /// Batch encodes multiple coordinate pairs for high-throughput scenarios.
        /// </summary>
        public static string[] BatchEncode(ReadOnlySpan<(double lat, double lon)> coordinates)
        {
            var results = new string[coordinates.Length];
            
            for (int i = 0; i < coordinates.Length; i++)
            {
                var (lat, lon) = coordinates[i];
                results[i] = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
            }
            
            return results;
        }

        /// <summary>
        /// Batch decodes multiple encoded strings for high-throughput scenarios.
        /// </summary>
        public static (double lat, double lon)[] BatchDecode(ReadOnlySpan<string> encoded)
        {
            var results = new (double, double)[encoded.Length];
            
            for (int i = 0; i < encoded.Length; i++)
            {
                results[i] = UniformPrecisionCoordinateCompressor.Decode(encoded[i]);
            }
            
            return results;
        }

        /// <summary>
        /// Finds all coordinates within a specified radius (in meters) of a center point.
        /// Returns encoded strings of nearby coordinates.
        /// </summary>
        public static string[] FindNearby(double centerLat, double centerLon, double radiusMeters, int maxResults = 100)
        {
            var results = new List<string>();
            string centerEncoded = UniformPrecisionCoordinateCompressor.Encode(centerLat, centerLon);
            
            // Calculate approximate grid search bounds
            double latDelta = radiusMeters / 111320.0; // Rough conversion
            double lonDelta = radiusMeters / (111320.0 * Math.Cos(centerLat * Math.PI / 180.0));
            
            double minLat = Math.Max(-80, centerLat - latDelta);
            double maxLat = Math.Min(80, centerLat + latDelta);
            double minLon = Math.Max(-180, centerLon - lonDelta);
            double maxLon = Math.Min(180, centerLon + lonDelta);
            
            // Grid search with 3-meter steps
            const double latStep = 3.0 / 111320.0;
            const double lonStep = 3.0 / 111320.0;
            
            for (double lat = minLat; lat <= maxLat && results.Count < maxResults; lat += latStep)
            {
                for (double lon = minLon; lon <= maxLon && results.Count < maxResults; lon += lonStep)
                {
                    try
                    {
                        string encoded = UniformPrecisionCoordinateCompressor.Encode(lat, lon);
                        double distance = UniformPrecisionCoordinateCompressor.CalculateDistance(centerEncoded, encoded);
                        
                        if (distance <= radiusMeters)
                        {
                            results.Add(encoded);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // Skip invalid coordinates (out of bounds, etc.)
                        continue;
                    }
                }
            }
            
            return results.ToArray();
        }
    }
}