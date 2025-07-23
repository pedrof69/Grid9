"""
Unit tests for MeterBasedCoordinateCompressor
"""

import pytest
from src import MeterBasedCoordinateCompressor


class TestMeterBasedCoordinateCompressor:
    """Test cases for MeterBasedCoordinateCompressor"""
    
    def test_encode_decode_basic(self):
        """Test basic encode/decode functionality"""
        # New York City coordinates
        lat, lon = 40.7128, -74.0060
        encoded = MeterBasedCoordinateCompressor.encode(lat, lon)
        
        # Should produce 9-character code
        assert len(encoded) == 9
        
        # Decode should return close coordinates
        decoded_lat, decoded_lon = MeterBasedCoordinateCompressor.decode(encoded)
        assert abs(decoded_lat - lat) < 0.001  # Within ~100m
        assert abs(decoded_lon - lon) < 0.001
    
    def test_human_readable_format(self):
        """Test human-readable formatting"""
        lat, lon = 40.7128, -74.0060
        
        # Test encoding with human-readable format
        encoded_readable = MeterBasedCoordinateCompressor.encode(lat, lon, human_readable=True)
        assert len(encoded_readable) == 11
        assert encoded_readable[3] == '-' and encoded_readable[7] == '-'
        
        # Test format conversion
        encoded_compact = MeterBasedCoordinateCompressor.encode(lat, lon, human_readable=False)
        formatted = MeterBasedCoordinateCompressor.format_for_humans(encoded_compact)
        assert formatted == encoded_readable
        
        # Test removing formatting
        unformatted = MeterBasedCoordinateCompressor.remove_formatting(formatted)
        assert unformatted == encoded_compact
    
    def test_edge_cases(self):
        """Test edge cases and boundary conditions"""
        # Test near poles (avoiding exact poles due to precision issues)
        encoded_north = MeterBasedCoordinateCompressor.encode(89.0, 0.0)
        encoded_south = MeterBasedCoordinateCompressor.encode(-89.0, 0.0)
        assert len(encoded_north) == 9
        assert len(encoded_south) == 9
        
        # Test date line
        encoded_east = MeterBasedCoordinateCompressor.encode(0.0, 179.9)
        encoded_west = MeterBasedCoordinateCompressor.encode(0.0, -179.9)
        assert len(encoded_east) == 9
        assert len(encoded_west) == 9
    
    def test_invalid_inputs(self):
        """Test invalid input handling"""
        # Out of range latitude
        with pytest.raises(ValueError):
            MeterBasedCoordinateCompressor.encode(91.0, 0.0)
        
        with pytest.raises(ValueError):
            MeterBasedCoordinateCompressor.encode(-91.0, 0.0)
        
        # Out of range longitude
        with pytest.raises(ValueError):
            MeterBasedCoordinateCompressor.encode(0.0, 181.0)
        
        with pytest.raises(ValueError):
            MeterBasedCoordinateCompressor.encode(0.0, -181.0)
    
    def test_precision_characteristics(self):
        """Test meter-based precision characteristics"""
        # At equator - best longitude precision
        lat_prec_eq, lon_prec_eq = MeterBasedCoordinateCompressor.get_theoretical_precision(0.0)
        
        # At 45 degrees - moderate longitude precision
        lat_prec_45, lon_prec_45 = MeterBasedCoordinateCompressor.get_theoretical_precision(45.0)
        
        # At 60 degrees - reduced longitude precision
        lat_prec_60, lon_prec_60 = MeterBasedCoordinateCompressor.get_theoretical_precision(60.0)
        
        # Latitude precision should be constant
        assert abs(lat_prec_eq - lat_prec_45) < 0.1
        assert abs(lat_prec_45 - lat_prec_60) < 0.1
        
        # Longitude precision should vary with latitude (gets better toward poles)
        assert lon_prec_eq > lon_prec_45 > lon_prec_60
    
    def test_actual_precision(self):
        """Test actual precision at various locations"""
        test_points = [
            (0.0, 0.0),    # Equator
            (45.0, 0.0),   # Mid-latitude
            (60.0, 0.0),   # High latitude
        ]
        
        for lat, lon in test_points:
            lat_error, lon_error, total_error = MeterBasedCoordinateCompressor.get_actual_precision(lat, lon)
            # Latitude error should be consistently low
            assert lat_error < 3.0, f"Latitude error at ({lat}, {lon}) exceeds 3m: {lat_error}m"
    
    def test_distance_calculation(self):
        """Test distance calculation between encoded points"""
        # Two nearby points
        point1 = MeterBasedCoordinateCompressor.encode(40.7128, -74.0060)
        point2 = MeterBasedCoordinateCompressor.encode(40.7580, -73.9855)
        
        distance = MeterBasedCoordinateCompressor.calculate_distance(point1, point2)
        # Should be approximately 7km
        assert 5000 < distance < 10000
    
    def test_neighbors(self):
        """Test neighbor generation"""
        encoded = MeterBasedCoordinateCompressor.encode(40.7128, -74.0060)
        neighbors = MeterBasedCoordinateCompressor.get_neighbors(encoded)
        
        # Should return up to 8 neighbors
        assert len(neighbors) <= 8
        assert len(neighbors) > 0
        
        # All neighbors should be valid encodings
        for neighbor in neighbors:
            assert MeterBasedCoordinateCompressor.is_valid_encoding(neighbor)
    
    def test_validation(self):
        """Test encoding validation"""
        # Generate a valid encoding
        valid_encoding = MeterBasedCoordinateCompressor.encode(40.7128, -74.0060)
        assert MeterBasedCoordinateCompressor.is_valid_encoding(valid_encoding)
        
        # Test with formatting
        formatted = MeterBasedCoordinateCompressor.format_for_humans(valid_encoding)
        assert MeterBasedCoordinateCompressor.is_valid_encoding(formatted)
        
        # Invalid encodings
        assert not MeterBasedCoordinateCompressor.is_valid_encoding("")
        assert not MeterBasedCoordinateCompressor.is_valid_encoding("ABC")  # Too short
        assert not MeterBasedCoordinateCompressor.is_valid_encoding("ABCDEFGHIJ")  # Too long
        assert not MeterBasedCoordinateCompressor.is_valid_encoding("ABCIEFGHI")  # Invalid char 'I'
    
    def test_polar_region_handling(self):
        """Test special handling of polar regions"""
        # Near north pole
        encoded_near_pole = MeterBasedCoordinateCompressor.encode(89.5, 45.0)
        decoded_lat, decoded_lon = MeterBasedCoordinateCompressor.decode(encoded_near_pole)
        
        # Should decode successfully
        assert abs(decoded_lat - 89.5) < 0.1
        assert -180 <= decoded_lon <= 180