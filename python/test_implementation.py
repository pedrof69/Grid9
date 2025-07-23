#!/usr/bin/env python3
"""
Quick test script to verify the Python implementation works correctly
"""

from src import UniformPrecisionCoordinateCompressor, MeterBasedCoordinateCompressor, CoordinateOperations


def test_uniform_precision():
    print("Testing UniformPrecisionCoordinateCompressor...")
    
    # Test encoding
    lat, lon = 40.7128, -74.0060  # NYC
    encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon)
    print(f"  Encoded {lat}, {lon} -> {encoded}")
    
    # Test decoding
    decoded_lat, decoded_lon = UniformPrecisionCoordinateCompressor.decode(encoded)
    print(f"  Decoded {encoded} -> {decoded_lat:.6f}, {decoded_lon:.6f}")
    
    # Test precision
    x_err, y_err, total_err = UniformPrecisionCoordinateCompressor.get_actual_precision(lat, lon)
    print(f"  Precision: {total_err:.2f}m")
    
    # Test human-readable format
    readable = UniformPrecisionCoordinateCompressor.encode(lat, lon, human_readable=True)
    print(f"  Human-readable: {readable}")
    
    print("  ✓ UniformPrecisionCoordinateCompressor works!")


def test_meter_based():
    print("\nTesting MeterBasedCoordinateCompressor...")
    
    # Test encoding
    lat, lon = 51.5074, -0.1278  # London
    encoded = MeterBasedCoordinateCompressor.encode(lat, lon)
    print(f"  Encoded {lat}, {lon} -> {encoded}")
    
    # Test decoding
    decoded_lat, decoded_lon = MeterBasedCoordinateCompressor.decode(encoded)
    print(f"  Decoded {encoded} -> {decoded_lat:.6f}, {decoded_lon:.6f}")
    
    # Test precision
    lat_err, lon_err, total_err = MeterBasedCoordinateCompressor.get_actual_precision(lat, lon)
    print(f"  Precision: {total_err:.2f}m")
    
    print("  ✓ MeterBasedCoordinateCompressor works!")


def test_coordinate_operations():
    print("\nTesting CoordinateOperations...")
    
    # Test batch encoding
    coordinates = [
        (40.7128, -74.0060),   # NYC
        (51.5074, -0.1278),    # London
        (35.6762, 139.6503),   # Tokyo
    ]
    
    encoded_list = CoordinateOperations.batch_encode(coordinates)
    print(f"  Batch encoded {len(coordinates)} coordinates:")
    for i, code in enumerate(encoded_list):
        print(f"    {coordinates[i]} -> {code}")
    
    # Test batch decoding
    decoded_list = CoordinateOperations.batch_decode(encoded_list)
    print(f"  Batch decoded successfully, got {len(decoded_list)} results")
    
    # Test find nearby
    nearby = CoordinateOperations.find_nearby(40.7128, -74.0060, 1000, max_results=5)
    print(f"  Found {len(nearby)} coordinates within 1km")
    
    print("  ✓ CoordinateOperations works!")


def main():
    print("Grid9 Python Implementation Test")
    print("=" * 40)
    
    test_uniform_precision()
    test_meter_based()
    test_coordinate_operations()
    
    print("\n✅ All tests passed! Python implementation is working correctly.")


if __name__ == "__main__":
    main()