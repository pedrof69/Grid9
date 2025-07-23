"""
Unit tests for CoordinateOperations
"""

import pytest
from src import CoordinateOperations, UniformPrecisionCoordinateCompressor


class TestCoordinateOperations:
    """Test cases for CoordinateOperations"""
    
    def test_batch_encode(self):
        """Test batch encoding of coordinates"""
        coordinates = [
            (40.7128, -74.0060),   # NYC
            (51.5074, -0.1278),    # London
            (35.6762, 139.6503),   # Tokyo
            (-33.8688, 151.2093),  # Sydney
        ]
        
        encoded = CoordinateOperations.batch_encode(coordinates)
        
        # Should return same number of results
        assert len(encoded) == len(coordinates)
        
        # All should be 9-character codes
        for code in encoded:
            assert len(code) == 9
            assert UniformPrecisionCoordinateCompressor.is_valid_encoding(code)
    
    def test_batch_decode(self):
        """Test batch decoding of encoded strings"""
        # First encode some coordinates
        coordinates = [
            (40.7128, -74.0060),
            (51.5074, -0.1278),
            (35.6762, 139.6503),
        ]
        encoded = CoordinateOperations.batch_encode(coordinates)
        
        # Now decode them
        decoded = CoordinateOperations.batch_decode(encoded)
        
        # Should return same number of results
        assert len(decoded) == len(encoded)
        
        # Check that decoded values are close to originals
        for i, (lat, lon) in enumerate(decoded):
            orig_lat, orig_lon = coordinates[i]
            assert abs(lat - orig_lat) < 0.001
            assert abs(lon - orig_lon) < 0.001
    
    def test_find_nearby_basic(self):
        """Test finding nearby coordinates"""
        center_lat, center_lon = 40.7128, -74.0060  # NYC
        radius_meters = 1000  # 1km radius
        
        nearby = CoordinateOperations.find_nearby(center_lat, center_lon, radius_meters)
        
        # Should find some results
        assert len(nearby) > 0
        
        # All results should be within radius
        center_encoded = UniformPrecisionCoordinateCompressor.encode(center_lat, center_lon)
        for encoded in nearby:
            distance = UniformPrecisionCoordinateCompressor.calculate_distance(center_encoded, encoded)
            assert distance <= radius_meters
    
    def test_find_nearby_max_results(self):
        """Test max_results parameter"""
        center_lat, center_lon = 40.7128, -74.0060
        radius_meters = 10000  # 10km radius (should have many results)
        max_results = 10
        
        nearby = CoordinateOperations.find_nearby(center_lat, center_lon, radius_meters, max_results)
        
        # Should respect max_results limit
        assert len(nearby) <= max_results
    
    def test_find_nearby_edge_cases(self):
        """Test find_nearby with edge cases"""
        # Near north pole
        nearby_pole = CoordinateOperations.find_nearby(89.0, 0.0, 1000)
        assert isinstance(nearby_pole, list)
        
        # Near date line
        nearby_dateline = CoordinateOperations.find_nearby(0.0, 179.5, 1000)
        assert isinstance(nearby_dateline, list)
        
        # Very small radius
        nearby_small = CoordinateOperations.find_nearby(40.7128, -74.0060, 10)  # 10m
        assert isinstance(nearby_small, list)
    
    def test_empty_batch_operations(self):
        """Test batch operations with empty inputs"""
        # Empty encode
        encoded = CoordinateOperations.batch_encode([])
        assert encoded == []
        
        # Empty decode
        decoded = CoordinateOperations.batch_decode([])
        assert decoded == []
    
    def test_performance_batch_vs_single(self):
        """Test that batch operations work correctly (not testing actual performance)"""
        # Generate many coordinates
        import random
        random.seed(42)
        
        coordinates = []
        for _ in range(100):
            lat = random.uniform(-80, 80)
            lon = random.uniform(-180, 180)
            coordinates.append((lat, lon))
        
        # Batch encode
        batch_encoded = CoordinateOperations.batch_encode(coordinates)
        
        # Single encode for comparison
        single_encoded = []
        for lat, lon in coordinates:
            single_encoded.append(UniformPrecisionCoordinateCompressor.encode(lat, lon))
        
        # Results should be identical
        assert batch_encoded == single_encoded