"""
High-performance coordinate operations with batch processing capabilities.
"""

import math
from typing import List, Tuple

from .uniform_precision_compressor import UniformPrecisionCoordinateCompressor


class CoordinateOperations:
    """High-performance coordinate operations with batch processing capabilities."""
    
    @staticmethod
    def batch_encode(coordinates: List[Tuple[float, float]]) -> List[str]:
        """
        Batch encodes multiple coordinate pairs for high-throughput scenarios.
        
        Args:
            coordinates: List of (latitude, longitude) tuples
            
        Returns:
            List of encoded strings
        """
        results = []
        
        for lat, lon in coordinates:
            results.append(UniformPrecisionCoordinateCompressor.encode(lat, lon))
        
        return results
    
    @staticmethod
    def batch_decode(encoded: List[str]) -> List[Tuple[float, float]]:
        """
        Batch decodes multiple encoded strings for high-throughput scenarios.
        
        Args:
            encoded: List of encoded strings
            
        Returns:
            List of (latitude, longitude) tuples
        """
        results = []
        
        for enc_str in encoded:
            results.append(UniformPrecisionCoordinateCompressor.decode(enc_str))
        
        return results
    
    @staticmethod
    def find_nearby(center_lat: float, center_lon: float, radius_meters: float, max_results: int = 100) -> List[str]:
        """
        Finds all coordinates within a specified radius (in meters) of a center point.
        Returns encoded strings of nearby coordinates.
        
        Args:
            center_lat: Center latitude in degrees
            center_lon: Center longitude in degrees
            radius_meters: Search radius in meters
            max_results: Maximum number of results to return (default: 100)
            
        Returns:
            List of encoded coordinate strings within the radius
        """
        results = []
        center_encoded = UniformPrecisionCoordinateCompressor.encode(center_lat, center_lon)
        
        # Calculate approximate grid search bounds
        lat_delta = radius_meters / 111320.0  # Rough conversion
        lon_delta = radius_meters / (111320.0 * math.cos(math.radians(center_lat)))
        
        min_lat = max(-80, center_lat - lat_delta)
        max_lat = min(80, center_lat + lat_delta)
        min_lon = max(-180, center_lon - lon_delta)
        max_lon = min(180, center_lon + lon_delta)
        
        # Grid search with 3-meter steps
        lat_step = 3.0 / 111320.0
        lon_step = 3.0 / 111320.0
        
        lat = min_lat
        while lat <= max_lat and len(results) < max_results:
            lon = min_lon
            while lon <= max_lon and len(results) < max_results:
                try:
                    encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon)
                    distance = UniformPrecisionCoordinateCompressor.calculate_distance(center_encoded, encoded)
                    
                    if distance <= radius_meters:
                        results.append(encoded)
                except ValueError:
                    # Skip invalid coordinates (out of bounds, etc.)
                    pass
                
                lon += lon_step
            lat += lat_step
        
        return results