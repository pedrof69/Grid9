#ifndef COORDINATE_OPERATIONS_H
#define COORDINATE_OPERATIONS_H

#include "UniformPrecisionCoordinateCompressor.h"
#include <vector>
#include <string>

namespace grid9 {

struct Coordinate {
    double lat;
    double lon;
    
    Coordinate(double latitude, double longitude) : lat(latitude), lon(longitude) {}
};

struct BoundingBox {
    double minLat;
    double maxLat;
    double minLon;
    double maxLon;
    
    BoundingBox(double minLatitude, double maxLatitude, double minLongitude, double maxLongitude)
        : minLat(minLatitude), maxLat(maxLatitude), minLon(minLongitude), maxLon(maxLongitude) {}
};

class CoordinateOperations {
public:
    static std::vector<std::string> batchEncode(const std::vector<Coordinate>& coordinates, bool humanReadable = false);
    static std::vector<Coordinate> batchDecode(const std::vector<std::string>& encoded);
    static std::vector<std::string> findNearby(double centerLat, double centerLon, double radiusMeters, int maxResults = 100);
    static BoundingBox getBoundingBox(const std::vector<Coordinate>& coordinates);
    static Coordinate getCenterPoint(const std::vector<Coordinate>& coordinates);
};

} // namespace grid9

#endif // COORDINATE_OPERATIONS_H