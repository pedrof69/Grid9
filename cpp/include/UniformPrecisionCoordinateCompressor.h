#ifndef UNIFORM_PRECISION_COORDINATE_COMPRESSOR_H
#define UNIFORM_PRECISION_COORDINATE_COMPRESSOR_H

#include <string>
#include <utility>

namespace grid9 {

class UniformPrecisionCoordinateCompressor {
public:
    static std::string encode(double latitude, double longitude, bool humanReadable = false);
    static std::pair<double, double> decode(const std::string& encoded);
    static double calculateDistance(const std::string& encoded1, const std::string& encoded2);
    static bool isValidEncoding(const std::string& encoded);
    static std::string formatForHumans(const std::string& encoded);
    static std::string removeFormatting(const std::string& formatted);
    
    struct PrecisionInfo {
        double latErrorM;
        double lonErrorM;
        double totalErrorM;
    };
    
    static PrecisionInfo getActualPrecision(double latitude, double longitude);

private:
    static const std::string BASE32_ALPHABET;
    static const int LAT_BITS = 22;
    static const int LON_BITS = 23;
    static const long long LAT_MAX = (1LL << LAT_BITS) - 1;
    static const long long LON_MAX = (1LL << LON_BITS) - 1;
    
    static double degreesToRadians(double degrees);
    static double radiansToDegrees(double radians);
    static double haversineDistance(double lat1, double lon1, double lat2, double lon2);
    static void validateCoordinates(double latitude, double longitude);
    static void validateEncodedString(const std::string& encoded);
};

} // namespace grid9

#endif // UNIFORM_PRECISION_COORDINATE_COMPRESSOR_H