#include "../include/UniformPrecisionCoordinateCompressor.h"
#include "../include/CoordinateOperations.h"
#include <iostream>
#include <iomanip>
#include <vector>

using namespace grid9;

int main() {
    std::cout << "=== Grid9 C++ Implementation Demo ===" << std::endl;
    std::cout << std::endl;
    
    // Test coordinates: NYC, London, Tokyo
    std::vector<std::pair<double, double>> testCoords = {
        {40.7128, -74.0060}, // NYC
        {51.5074, -0.1278},  // London
        {35.6762, 139.6503}  // Tokyo
    };
    
    std::vector<std::string> cityNames = {"New York", "London", "Tokyo"};
    
    std::cout << "=== Basic Encoding/Decoding ===" << std::endl;
    for (size_t i = 0; i < testCoords.size(); i++) {
        double lat = testCoords[i].first;
        double lon = testCoords[i].second;
        
        // Encode
        std::string compact = UniformPrecisionCoordinateCompressor::encode(lat, lon);
        std::string readable = UniformPrecisionCoordinateCompressor::encode(lat, lon, true);
        
        // Decode
        auto decoded = UniformPrecisionCoordinateCompressor::decode(compact);
        
        // Precision info
        auto precision = UniformPrecisionCoordinateCompressor::getActualPrecision(lat, lon);
        
        std::cout << cityNames[i] << ":" << std::endl;
        std::cout << "  Original: (" << std::fixed << std::setprecision(6) << lat << ", " << lon << ")" << std::endl;
        std::cout << "  Compact:  " << compact << std::endl;
        std::cout << "  Readable: " << readable << std::endl;
        std::cout << "  Decoded:  (" << std::fixed << std::setprecision(6) << decoded.first << ", " << decoded.second << ")" << std::endl;
        std::cout << "  Precision: " << std::fixed << std::setprecision(1) << precision.totalErrorM << "m total" << std::endl;
        std::cout << std::endl;
    }
    
    std::cout << "=== Distance Calculation ===" << std::endl;
    std::string nycCode = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060);
    std::string londonCode = UniformPrecisionCoordinateCompressor::encode(51.5074, -0.1278);
    
    double distance = UniformPrecisionCoordinateCompressor::calculateDistance(nycCode, londonCode);
    std::cout << "Distance NYC to London: " << std::fixed << std::setprecision(0) << distance << " meters" << std::endl;
    std::cout << std::endl;
    
    std::cout << "=== Batch Operations ===" << std::endl;
    std::vector<Coordinate> coordinates;
    for (const auto& coord : testCoords) {
        coordinates.emplace_back(coord.first, coord.second);
    }
    
    auto encodedBatch = CoordinateOperations::batchEncode(coordinates);
    auto decodedBatch = CoordinateOperations::batchDecode(encodedBatch);
    
    std::cout << "Batch encoded " << encodedBatch.size() << " coordinates:" << std::endl;
    for (size_t i = 0; i < encodedBatch.size(); i++) {
        std::cout << "  " << cityNames[i] << ": " << encodedBatch[i] << std::endl;
    }
    std::cout << std::endl;
    
    std::cout << "=== Validation ===" << std::endl;
    std::vector<std::string> testStrings = {
        "Q7KH2BBYF",      // Valid
        "Q7K-H2B-BYF",    // Valid with dashes
        "INVALID123",     // Invalid
        "TOOLONGSTRING"   // Invalid length
    };
    
    for (const auto& testStr : testStrings) {
        bool isValid = UniformPrecisionCoordinateCompressor::isValidEncoding(testStr);
        std::cout << "\"" << testStr << "\" is " << (isValid ? "valid" : "invalid") << std::endl;
    }
    std::cout << std::endl;
    
    std::cout << "=== Nearby Search ===" << std::endl;
    double centerLat = 40.7128; // NYC
    double centerLon = -74.0060;
    double radius = 1000; // 1km
    
    auto nearby = CoordinateOperations::findNearby(centerLat, centerLon, radius, 5);
    std::cout << "Found " << nearby.size() << " points within " << radius << "m of NYC:" << std::endl;
    for (const auto& code : nearby) {
        auto coord = UniformPrecisionCoordinateCompressor::decode(code);
        std::cout << "  " << code << " -> (" << std::fixed << std::setprecision(6) 
                  << coord.first << ", " << coord.second << ")" << std::endl;
    }
    
    std::cout << std::endl << "Demo completed successfully!" << std::endl;
    return 0;
}