#include "../include/UniformPrecisionCoordinateCompressor.h"
#include <cmath>
#include <stdexcept>
#include <sstream>
#include <algorithm>

namespace grid9 {

const std::string UniformPrecisionCoordinateCompressor::BASE32_ALPHABET = "0123456789ABCDEFGHJKMNPQRSTVWXYZ";

std::string UniformPrecisionCoordinateCompressor::encode(double latitude, double longitude, bool humanReadable) {
    validateCoordinates(latitude, longitude);
    
    // Normalize coordinates to [0, 1] range
    double normLat = (latitude + 90.0) / 180.0;
    double normLon = (longitude + 180.0) / 360.0;
    
    // Quantize to bit precision
    long long latBits = static_cast<long long>(normLat * LAT_MAX);
    long long lonBits = static_cast<long long>(normLon * LON_MAX);
    
    // Ensure within bounds
    latBits = std::min(latBits, LAT_MAX);
    lonBits = std::min(lonBits, LON_MAX);
    
    // Pack into 45-bit value
    long long packed = (latBits << LON_BITS) | lonBits;
    
    // Convert to base32 (9 characters for 45 bits)
    std::string result;
    result.reserve(9);
    
    for (int i = 0; i < 9; i++) {
        int index = static_cast<int>(packed & 0x1F);
        result = BASE32_ALPHABET[index] + result;
        packed >>= 5;
    }
    
    if (humanReadable) {
        return formatForHumans(result);
    }
    
    return result;
}

std::pair<double, double> UniformPrecisionCoordinateCompressor::decode(const std::string& encoded) {
    std::string cleanEncoded = removeFormatting(encoded);
    validateEncodedString(cleanEncoded);
    
    // Convert from base32 to 45-bit value
    long long packed = 0;
    for (char c : cleanEncoded) {
        packed <<= 5;
        auto pos = BASE32_ALPHABET.find(c);
        if (pos == std::string::npos) {
            throw std::invalid_argument("Invalid character in encoded string: " + std::string(1, c));
        }
        packed |= pos;
    }
    
    // Extract latitude and longitude bits
    long long lonBits = packed & LON_MAX;
    long long latBits = (packed >> LON_BITS) & LAT_MAX;
    
    // Denormalize coordinates
    double normLat = static_cast<double>(latBits) / LAT_MAX;
    double normLon = static_cast<double>(lonBits) / LON_MAX;
    
    double latitude = normLat * 180.0 - 90.0;
    double longitude = normLon * 360.0 - 180.0;
    
    return std::make_pair(latitude, longitude);
}

double UniformPrecisionCoordinateCompressor::calculateDistance(const std::string& encoded1, const std::string& encoded2) {
    auto coord1 = decode(encoded1);
    auto coord2 = decode(encoded2);
    
    return haversineDistance(coord1.first, coord1.second, coord2.first, coord2.second);
}

bool UniformPrecisionCoordinateCompressor::isValidEncoding(const std::string& encoded) {
    try {
        validateEncodedString(removeFormatting(encoded));
        return true;
    } catch (const std::exception&) {
        return false;
    }
}

std::string UniformPrecisionCoordinateCompressor::formatForHumans(const std::string& encoded) {
    if (encoded.length() != 9) {
        throw std::invalid_argument("Encoded string must be exactly 9 characters");
    }
    
    return encoded.substr(0, 3) + "-" + encoded.substr(3, 3) + "-" + encoded.substr(6, 3);
}

std::string UniformPrecisionCoordinateCompressor::removeFormatting(const std::string& formatted) {
    std::string result;
    result.reserve(formatted.length());
    
    for (char c : formatted) {
        if (c != '-') {
            result += c;
        }
    }
    
    return result;
}

UniformPrecisionCoordinateCompressor::PrecisionInfo 
UniformPrecisionCoordinateCompressor::getActualPrecision(double latitude, double longitude) {
    validateCoordinates(latitude, longitude);
    
    // Calculate precision based on latitude quantization
    double latPrecision = 180.0 / (1LL << LAT_BITS);
    double latErrorM = latPrecision * 111320.0;
    
    // Calculate precision based on longitude quantization and latitude
    double lonPrecision = 360.0 / (1LL << LON_BITS);
    double lonErrorM = lonPrecision * 111320.0 * std::cos(degreesToRadians(latitude));
    
    // Total error is the diagonal of the error rectangle
    double totalErrorM = std::sqrt(latErrorM * latErrorM + lonErrorM * lonErrorM);
    
    return {latErrorM, lonErrorM, totalErrorM};
}

double UniformPrecisionCoordinateCompressor::degreesToRadians(double degrees) {
    return degrees * M_PI / 180.0;
}

double UniformPrecisionCoordinateCompressor::radiansToDegrees(double radians) {
    return radians * 180.0 / M_PI;
}

double UniformPrecisionCoordinateCompressor::haversineDistance(double lat1, double lon1, double lat2, double lon2) {
    const double EARTH_RADIUS_M = 6371000.0;
    
    double dLat = degreesToRadians(lat2 - lat1);
    double dLon = degreesToRadians(lon2 - lon1);
    
    double a = std::sin(dLat / 2) * std::sin(dLat / 2) +
               std::cos(degreesToRadians(lat1)) * std::cos(degreesToRadians(lat2)) *
               std::sin(dLon / 2) * std::sin(dLon / 2);
    
    double c = 2 * std::atan2(std::sqrt(a), std::sqrt(1 - a));
    
    return EARTH_RADIUS_M * c;
}

void UniformPrecisionCoordinateCompressor::validateCoordinates(double latitude, double longitude) {
    if (latitude < -90.0 || latitude > 90.0) {
        throw std::invalid_argument("Latitude must be between -90 and 90 degrees");
    }
    if (longitude < -180.0 || longitude > 180.0) {
        throw std::invalid_argument("Longitude must be between -180 and 180 degrees");
    }
}

void UniformPrecisionCoordinateCompressor::validateEncodedString(const std::string& encoded) {
    if (encoded.length() != 9) {
        throw std::invalid_argument("Encoded string must be exactly 9 characters");
    }
    
    for (char c : encoded) {
        if (BASE32_ALPHABET.find(c) == std::string::npos) {
            throw std::invalid_argument("Invalid character in encoded string: " + std::string(1, c));
        }
    }
}

} // namespace grid9