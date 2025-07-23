#include "include/UniformPrecisionCoordinateCompressor.h"
#include "include/CoordinateOperations.h"
#include <iostream>
#include <cassert>
#include <cmath>

using namespace grid9;

void test_basic_functionality() {
    std::cout << "Testing basic encode/decode..." << std::endl;
    
    // Test NYC coordinates
    double lat = 40.7128;
    double lon = -74.0060;
    
    std::string encoded = UniformPrecisionCoordinateCompressor::encode(lat, lon);
    std::cout << "Encoded " << lat << ", " << lon << " to: " << encoded << std::endl;
    
    auto decoded = UniformPrecisionCoordinateCompressor::decode(encoded);
    std::cout << "Decoded back to: " << decoded.first << ", " << decoded.second << std::endl;
    
    // Should be within reasonable precision
    assert(std::abs(decoded.first - lat) < 0.01);
    assert(std::abs(decoded.second - lon) < 0.01);
    
    std::cout << "✓ Basic encode/decode test passed" << std::endl;
}

void test_human_readable() {
    std::cout << "\nTesting human-readable format..." << std::endl;
    
    std::string compact = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060);
    std::string readable = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060, true);
    
    std::cout << "Compact: " << compact << std::endl;
    std::cout << "Readable: " << readable << std::endl;
    
    // Should have dashes
    assert(readable.find('-') != std::string::npos);
    assert(readable.length() == 11); // 9 chars + 2 dashes
    
    // Should decode to same coordinates
    auto coord1 = UniformPrecisionCoordinateCompressor::decode(compact);
    auto coord2 = UniformPrecisionCoordinateCompressor::decode(readable);
    
    assert(std::abs(coord1.first - coord2.first) < 1e-10);
    assert(std::abs(coord1.second - coord2.second) < 1e-10);
    
    std::cout << "✓ Human-readable format test passed" << std::endl;
}

void test_validation() {
    std::cout << "\nTesting validation..." << std::endl;
    
    assert(UniformPrecisionCoordinateCompressor::isValidEncoding("Q7KH2BBYF"));
    assert(UniformPrecisionCoordinateCompressor::isValidEncoding("Q7K-H2B-BYF"));
    assert(!UniformPrecisionCoordinateCompressor::isValidEncoding("INVALID"));
    assert(!UniformPrecisionCoordinateCompressor::isValidEncoding("TOOLONG"));
    
    std::cout << "✓ Validation test passed" << std::endl;
}

void test_distance() {
    std::cout << "\nTesting distance calculation..." << std::endl;
    
    std::string nyc = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060);
    std::string london = UniformPrecisionCoordinateCompressor::encode(51.5074, -0.1278);
    
    double distance = UniformPrecisionCoordinateCompressor::calculateDistance(nyc, london);
    std::cout << "Distance NYC to London: " << distance << " meters" << std::endl;
    
    // Should be approximately 5,570 km
    assert(distance > 5500000.0 && distance < 5600000.0);
    
    std::cout << "✓ Distance calculation test passed" << std::endl;
}

void test_batch_operations() {
    std::cout << "\nTesting batch operations..." << std::endl;
    
    std::vector<Coordinate> coords = {
        Coordinate(40.7128, -74.0060), // NYC
        Coordinate(51.5074, -0.1278),  // London
        Coordinate(35.6762, 139.6503)  // Tokyo
    };
    
    auto encoded = CoordinateOperations::batchEncode(coords);
    auto decoded = CoordinateOperations::batchDecode(encoded);
    
    assert(encoded.size() == 3);
    assert(decoded.size() == 3);
    
    for (size_t i = 0; i < coords.size(); i++) {
        assert(std::abs(coords[i].lat - decoded[i].lat) < 0.01);
        assert(std::abs(coords[i].lon - decoded[i].lon) < 0.01);
    }
    
    std::cout << "✓ Batch operations test passed" << std::endl;
}

int main() {
    std::cout << "=== Grid9 C++ Implementation Tests ===" << std::endl;
    
    try {
        test_basic_functionality();
        test_human_readable();
        test_validation();
        test_distance();
        test_batch_operations();
        
        std::cout << "\n🎉 All tests passed!" << std::endl;
        return 0;
    } catch (const std::exception& e) {
        std::cerr << "❌ Test failed: " << e.what() << std::endl;
        return 1;
    }
}