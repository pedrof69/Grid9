"""
Unit tests for UniformPrecisionCoordinateCompressor
"""

import pytest
from src import UniformPrecisionCoordinateCompressor


class TestUniformPrecisionCoordinateCompressor:
    """Test cases for UniformPrecisionCoordinateCompressor"""
    
    def test_encode_decode_basic(self):
        """Test basic encode/decode functionality"""
        # New York City coordinates
        lat, lon = 40.7128, -74.0060
        encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon)
        
        # Should produce 9-character code
        assert len(encoded) == 9
        
        # Decode should return close coordinates
        decoded_lat, decoded_lon = UniformPrecisionCoordinateCompressor.decode(encoded)
        assert abs(decoded_lat - lat) < 0.001  # Within ~100m
        assert abs(decoded_lon - lon) < 0.001
    
    def test_human_readable_format(self):
        """Test human-readable formatting"""
        lat, lon = 40.7128, -74.0060
        
        # Test encoding with human-readable format
        encoded_readable = UniformPrecisionCoordinateCompressor.encode(lat, lon, human_readable=True)
        assert len(encoded_readable) == 11
        assert encoded_readable[3] == '-' and encoded_readable[7] == '-'
        
        # Test format conversion
        encoded_compact = UniformPrecisionCoordinateCompressor.encode(lat, lon, human_readable=False)
        formatted = UniformPrecisionCoordinateCompressor.format_for_humans(encoded_compact)
        assert formatted == encoded_readable
        
        # Test removing formatting
        unformatted = UniformPrecisionCoordinateCompressor.remove_formatting(formatted)
        assert unformatted == encoded_compact
    
    def test_edge_cases(self):
        """Test edge cases and boundary conditions"""
        # Test poles
        encoded_north = UniformPrecisionCoordinateCompressor.encode(90.0, 0.0)
        encoded_south = UniformPrecisionCoordinateCompressor.encode(-90.0, 0.0)
        assert len(encoded_north) == 9
        assert len(encoded_south) == 9
        
        # Test date line
        encoded_east = UniformPrecisionCoordinateCompressor.encode(0.0, 180.0)
        encoded_west = UniformPrecisionCoordinateCompressor.encode(0.0, -180.0)
        assert len(encoded_east) == 9
        assert len(encoded_west) == 9
    
    def test_invalid_inputs(self):
        """Test invalid input handling"""
        # Out of range latitude
        with pytest.raises(ValueError):
            UniformPrecisionCoordinateCompressor.encode(91.0, 0.0)
        
        with pytest.raises(ValueError):
            UniformPrecisionCoordinateCompressor.encode(-91.0, 0.0)
        
        # Out of range longitude
        with pytest.raises(ValueError):
            UniformPrecisionCoordinateCompressor.encode(0.0, 181.0)
        
        with pytest.raises(ValueError):
            UniformPrecisionCoordinateCompressor.encode(0.0, -181.0)
    
    def test_precision(self):
        """Test that precision is within expected bounds"""
        # Test at various latitudes
        test_points = [
            (0.0, 0.0),    # Equator
            (45.0, 0.0),   # Mid-latitude
            (60.0, 0.0),   # High latitude
            (80.0, 0.0),   # Near pole
        ]
        
        for lat, lon in test_points:
            x_error, y_error, total_error = UniformPrecisionCoordinateCompressor.get_actual_precision(lat, lon)
            # Should be under 4 meters globally
            assert total_error < 4.0, f"Precision at ({lat}, {lon}) exceeds 4m: {total_error}m"
    
    def test_distance_calculation(self):
        """Test distance calculation between encoded points"""
        # Two nearby points in NYC
        point1 = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060)
        point2 = UniformPrecisionCoordinateCompressor.encode(40.7580, -73.9855)  # Times Square
        
        distance = UniformPrecisionCoordinateCompressor.calculate_distance(point1, point2)
        # Should be approximately 7km
        assert 5000 < distance < 10000
    
    def test_neighbors(self):
        """Test neighbor generation"""
        encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060)
        neighbors = UniformPrecisionCoordinateCompressor.get_neighbors(encoded)
        
        # Should return up to 8 neighbors
        assert len(neighbors) <= 8
        assert len(neighbors) > 0
        
        # All neighbors should be valid encodings
        for neighbor in neighbors:
            assert UniformPrecisionCoordinateCompressor.is_valid_encoding(neighbor)
    
    def test_validation(self):
        """Test encoding validation"""
        # Valid encodings
        assert UniformPrecisionCoordinateCompressor.is_valid_encoding("Q7KH2BBYF")
        assert UniformPrecisionCoordinateCompressor.is_valid_encoding("Q7K-H2B-BYF")
        
        # Invalid encodings
        assert not UniformPrecisionCoordinateCompressor.is_valid_encoding("")
        assert not UniformPrecisionCoordinateCompressor.is_valid_encoding("ABC")  # Too short
        assert not UniformPrecisionCoordinateCompressor.is_valid_encoding("ABCDEFGHIJ")  # Too long
        assert not UniformPrecisionCoordinateCompressor.is_valid_encoding("ABC-DEF-GHI-JKL")  # Wrong format
        assert not UniformPrecisionCoordinateCompressor.is_valid_encoding("ABCIEFGHI")  # Invalid char 'I'