#!/usr/bin/env python3
"""
Simple validation test for Grid9 implementations.
This validates the core algorithm logic without requiring external dependencies.
"""

def validate_base32_alphabet():
    """Validate the base32 alphabet is consistent across implementations"""
    expected = "0123456789ABCDEFGHJKMNPQRSTVWXYZ"
    print(f"Expected Base32 alphabet: {expected}")
    print(f"Length: {len(expected)} characters")
    
    # Check no duplicates
    assert len(set(expected)) == len(expected), "Base32 alphabet has duplicates"
    
    # Check excludes I, L, O, U as documented
    excluded = set("ILOU")
    actual_chars = set(expected)
    assert not excluded.intersection(actual_chars), "Base32 alphabet contains excluded characters"
    
    print("✓ Base32 alphabet validation passed")

def validate_bit_allocation():
    """Validate the bit allocation strategy"""
    LAT_BITS = 22
    LON_BITS = 23
    TOTAL_BITS = LAT_BITS + LON_BITS
    CHARACTERS = 9
    BITS_PER_CHAR = 5
    
    print(f"Latitude bits: {LAT_BITS}")
    print(f"Longitude bits: {LON_BITS}") 
    print(f"Total bits: {TOTAL_BITS}")
    print(f"Characters: {CHARACTERS}")
    print(f"Bits per character: {BITS_PER_CHAR}")
    print(f"Total capacity: {CHARACTERS * BITS_PER_CHAR} bits")
    
    assert TOTAL_BITS == 45, f"Expected 45 total bits, got {TOTAL_BITS}"
    assert CHARACTERS * BITS_PER_CHAR == 45, "Character capacity doesn't match bit allocation"
    
    # Validate precision calculations
    lat_precision_deg = 180.0 / (2**LAT_BITS)
    lon_precision_deg = 360.0 / (2**LON_BITS)
    
    # At equator (worst case for longitude)
    lat_precision_m = lat_precision_deg * 111320.0
    lon_precision_m_equator = lon_precision_deg * 111320.0
    
    print(f"Latitude precision: {lat_precision_deg:.8f}° = {lat_precision_m:.2f}m")
    print(f"Longitude precision at equator: {lon_precision_deg:.8f}° = {lon_precision_m_equator:.2f}m")
    
    # Should be under 5 meters for both
    assert lat_precision_m < 5.0, f"Latitude precision too low: {lat_precision_m}m"
    assert lon_precision_m_equator < 5.0, f"Longitude precision too low: {lon_precision_m_equator}m"
    
    print("✓ Bit allocation validation passed")

def validate_coordinate_bounds():
    """Validate coordinate boundary handling"""
    # Test boundary coordinates
    boundary_coords = [
        (90.0, 180.0),   # North Pole, Date Line
        (-90.0, -180.0), # South Pole, Date Line  
        (0.0, 0.0),      # Equator, Prime Meridian
        (89.9, 179.9),   # Near boundaries
        (-89.9, -179.9), # Near boundaries
    ]
    
    LAT_BITS = 22
    LON_BITS = 23
    LAT_MAX = (2**LAT_BITS) - 1
    LON_MAX = (2**LON_BITS) - 1
    
    print("Testing coordinate normalization...")
    for lat, lon in boundary_coords:
        # Normalize coordinates to [0, 1] range
        norm_lat = (lat + 90.0) / 180.0
        norm_lon = (lon + 180.0) / 360.0
        
        # Quantize to bit precision
        lat_bits = min(int(norm_lat * LAT_MAX), LAT_MAX)
        lon_bits = min(int(norm_lon * LON_MAX), LON_MAX)
        
        # Should be within valid range
        assert 0 <= lat_bits <= LAT_MAX, f"Latitude bits out of range: {lat_bits}"
        assert 0 <= lon_bits <= LON_MAX, f"Longitude bits out of range: {lon_bits}"
        
        print(f"  ({lat:6.1f}, {lon:7.1f}) -> norm({norm_lat:.6f}, {norm_lon:.6f}) -> bits({lat_bits}, {lon_bits})")
    
    print("✓ Coordinate bounds validation passed")

def validate_encoding_decoding():
    """Validate basic encoding/decoding logic"""
    BASE32_ALPHABET = "0123456789ABCDEFGHJKMNPQRSTVWXYZ"
    LAT_BITS = 22
    LON_BITS = 23
    LAT_MAX = (2**LAT_BITS) - 1
    LON_MAX = (2**LON_BITS) - 1
    
    # Test NYC coordinates
    lat, lon = 40.7128, -74.0060
    
    print(f"Testing encode/decode for NYC: ({lat}, {lon})")
    
    # Encode
    norm_lat = (lat + 90.0) / 180.0
    norm_lon = (lon + 180.0) / 360.0
    
    lat_bits = min(int(norm_lat * LAT_MAX), LAT_MAX)
    lon_bits = min(int(norm_lon * LON_MAX), LON_MAX)
    
    # Pack into 45-bit value
    packed = (lat_bits << LON_BITS) | lon_bits
    
    # Convert to base32
    encoded_chars = []
    temp = packed
    for i in range(9):
        index = temp & 0x1F
        encoded_chars.insert(0, BASE32_ALPHABET[index])
        temp >>= 5
    
    encoded = ''.join(encoded_chars)
    print(f"Encoded: {encoded}")
    
    # Decode
    decoded_packed = 0
    for char in encoded:
        decoded_packed <<= 5
        index = BASE32_ALPHABET.index(char)
        decoded_packed |= index
    
    # Extract bits
    decoded_lon_bits = decoded_packed & LON_MAX
    decoded_lat_bits = (decoded_packed >> LON_BITS) & LAT_MAX
    
    # Denormalize
    decoded_norm_lat = decoded_lat_bits / LAT_MAX
    decoded_norm_lon = decoded_lon_bits / LON_MAX
    
    decoded_lat = decoded_norm_lat * 180.0 - 90.0
    decoded_lon = decoded_norm_lon * 360.0 - 180.0
    
    print(f"Decoded: ({decoded_lat:.6f}, {decoded_lon:.6f})")
    
    # Should be within reasonable precision
    lat_error = abs(decoded_lat - lat)
    lon_error = abs(decoded_lon - lon)
    
    print(f"Error: lat={lat_error:.6f}°, lon={lon_error:.6f}°")
    
    assert lat_error < 0.01, f"Latitude error too large: {lat_error}"
    assert lon_error < 0.01, f"Longitude error too large: {lon_error}"
    
    print("✓ Encoding/decoding validation passed")

def main():
    """Run all validation tests"""
    print("=== Grid9 Implementation Validation ===\n")
    
    try:
        validate_base32_alphabet()
        print()
        
        validate_bit_allocation()
        print()
        
        validate_coordinate_bounds()
        print()
        
        validate_encoding_decoding()
        print()
        
        print("🎉 All validation tests passed!")
        print("\nThe Grid9 implementations should work correctly with these parameters.")
        
    except AssertionError as e:
        print(f"❌ Validation failed: {e}")
        return 1
    except Exception as e:
        print(f"❌ Unexpected error: {e}")
        return 1
    
    return 0

if __name__ == "__main__":
    exit(main())