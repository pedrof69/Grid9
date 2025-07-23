#include "../include/CoordinateOperations.h"
#include <algorithm>
#include <cmath>
#include <stdexcept>

namespace grid9 {

std::vector<std::string> CoordinateOperations::batchEncode(const std::vector<Coordinate>& coordinates, bool humanReadable) {
    std::vector<std::string> results;
    results.reserve(coordinates.size());
    
    for (const auto& coord : coordinates) {
        results.push_back(UniformPrecisionCoordinateCompressor::encode(coord.lat, coord.lon, humanReadable));
    }
    
    return results;
}

std::vector<Coordinate> CoordinateOperations::batchDecode(const std::vector<std::string>& encoded) {
    std::vector<Coordinate> results;
    results.reserve(encoded.size());
    
    for (const auto& enc : encoded) {
        auto decoded = UniformPrecisionCoordinateCompressor::decode(enc);
        results.emplace_back(decoded.first, decoded.second);
    }
    
    return results;
}

std::vector<std::string> CoordinateOperations::findNearby(double centerLat, double centerLon, double radiusMeters, int maxResults) {
    if (radiusMeters <= 0) {
        throw std::invalid_argument("Radius must be positive");
    }
    if (maxResults <= 0) {
        throw std::invalid_argument("Max results must be positive");
    }
    
    std::vector<std::string> results;
    results.reserve(maxResults);
    
    std::string centerEncoded = UniformPrecisionCoordinateCompressor::encode(centerLat, centerLon);
    
    // Calculate approximate grid search bounds
    double latDelta = radiusMeters / 111320.0; // Rough conversion
    double lonDelta = radiusMeters / (111320.0 * std::cos(centerLat * M_PI / 180.0));
    
    double minLat = std::max(-80.0, centerLat - latDelta);
    double maxLat = std::min(80.0, centerLat + latDelta);
    double minLon = std::max(-180.0, centerLon - lonDelta);
    double maxLon = std::min(180.0, centerLon + lonDelta);
    
    // Grid search with 3-meter steps
    double latStep = 3.0 / 111320.0;
    double lonStep = 3.0 / 111320.0;
    
    for (double lat = minLat; lat <= maxLat && static_cast<int>(results.size()) < maxResults; lat += latStep) {
        for (double lon = minLon; lon <= maxLon && static_cast<int>(results.size()) < maxResults; lon += lonStep) {
            try {
                std::string encoded = UniformPrecisionCoordinateCompressor::encode(lat, lon);
                double distance = UniformPrecisionCoordinateCompressor::calculateDistance(centerEncoded, encoded);
                
                if (distance <= radiusMeters) {
                    results.push_back(encoded);
                }
            } catch (const std::exception&) {
                // Skip invalid coordinates (out of bounds, etc.)
                continue;
            }
        }
    }
    
    return results;
}

BoundingBox CoordinateOperations::getBoundingBox(const std::vector<Coordinate>& coordinates) {
    if (coordinates.empty()) {
        throw std::invalid_argument("Coordinates vector cannot be empty");
    }
    
    double minLat = coordinates[0].lat;
    double maxLat = coordinates[0].lat;
    double minLon = coordinates[0].lon;
    double maxLon = coordinates[0].lon;
    
    for (const auto& coord : coordinates) {
        minLat = std::min(minLat, coord.lat);
        maxLat = std::max(maxLat, coord.lat);
        minLon = std::min(minLon, coord.lon);
        maxLon = std::max(maxLon, coord.lon);
    }
    
    return BoundingBox(minLat, maxLat, minLon, maxLon);
}

Coordinate CoordinateOperations::getCenterPoint(const std::vector<Coordinate>& coordinates) {
    if (coordinates.empty()) {
        throw std::invalid_argument("Coordinates vector cannot be empty");
    }
    
    double totalLat = 0.0;
    double totalLon = 0.0;
    
    for (const auto& coord : coordinates) {
        totalLat += coord.lat;
        totalLon += coord.lon;
    }
    
    return Coordinate(totalLat / coordinates.size(), totalLon / coordinates.size());
}

} // namespace grid9