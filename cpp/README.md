# Grid9 C++ Implementation

High-performance C++ implementation of the Grid9 coordinate compression system with **9-character codes** providing **uniform 3-meter precision globally**.

## Features

- **Header-only option** - Include and use directly
- **CMake support** - Easy integration into existing projects
- **C++11 compatible** - Works with older compilers
- **High performance** - Optimized for speed and minimal memory allocation
- **Exception safety** - Proper error handling with standard exceptions
- **STL integration** - Uses standard containers and algorithms

## Quick Start

### Building with CMake

```bash
# Clone the repository
git clone https://github.com/pedrof69/Grid9.git
cd Grid9/cpp

# Build
mkdir build && cd build
cmake ..
make

# Run demo
./grid9_demo

# Run tests (if Google Test is available)
make test
```

### Basic Usage

```cpp
#include "UniformPrecisionCoordinateCompressor.h"
#include "CoordinateOperations.h"
#include <iostream>

using namespace grid9;

int main() {
    // Encode coordinates
    std::string code = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060);
    std::cout << "NYC: " << code << std::endl; // Output: Q7KH2BBYF
    
    // Human-readable format
    std::string readable = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060, true);
    std::cout << "NYC readable: " << readable << std::endl; // Output: Q7K-H2B-BYF
    
    // Decode coordinates
    auto coords = UniformPrecisionCoordinateCompressor::decode(code);
    std::cout << "Decoded: (" << coords.first << ", " << coords.second << ")" << std::endl;
    
    // Calculate distance between two codes
    std::string londonCode = UniformPrecisionCoordinateCompressor::encode(51.5074, -0.1278);
    double distance = UniformPrecisionCoordinateCompressor::calculateDistance(code, londonCode);
    std::cout << "Distance NYC to London: " << distance << " meters" << std::endl;
    
    return 0;
}
```

### Batch Operations

```cpp
#include "CoordinateOperations.h"
#include <vector>

// Batch encoding
std::vector<Coordinate> coordinates = {
    {40.7128, -74.0060}, // NYC
    {51.5074, -0.1278},  // London
    {35.6762, 139.6503}  // Tokyo
};

auto encoded = CoordinateOperations::batchEncode(coordinates);
auto decoded = CoordinateOperations::batchDecode(encoded);

// Find nearby points
auto nearby = CoordinateOperations::findNearby(40.7128, -74.0060, 1000.0, 10);
```

## API Reference

### UniformPrecisionCoordinateCompressor

#### Static Methods

- `std::string encode(double latitude, double longitude, bool humanReadable = false)`
  - Encodes coordinates to Grid9 string
  - Returns 9-character code or XXX-XXX-XXX format if humanReadable is true

- `std::pair<double, double> decode(const std::string& encoded)`
  - Decodes Grid9 string to coordinates
  - Returns pair of (latitude, longitude)

- `double calculateDistance(const std::string& encoded1, const std::string& encoded2)`
  - Calculates distance between two Grid9 codes in meters

- `bool isValidEncoding(const std::string& encoded)`
  - Validates Grid9 encoding format

- `PrecisionInfo getActualPrecision(double latitude, double longitude)`
  - Returns precision information for given coordinates

#### Utility Methods

- `std::string formatForHumans(const std::string& encoded)`
  - Converts compact format to XXX-XXX-XXX

- `std::string removeFormatting(const std::string& formatted)`
  - Removes dashes from formatted string

### CoordinateOperations

#### Batch Operations

- `std::vector<std::string> batchEncode(const std::vector<Coordinate>& coordinates, bool humanReadable = false)`
- `std::vector<Coordinate> batchDecode(const std::vector<std::string>& encoded)`

#### Spatial Operations

- `std::vector<std::string> findNearby(double centerLat, double centerLon, double radiusMeters, int maxResults = 100)`
- `BoundingBox getBoundingBox(const std::vector<Coordinate>& coordinates)`
- `Coordinate getCenterPoint(const std::vector<Coordinate>& coordinates)`

## Integration

### CMake Integration

Add Grid9 to your CMake project:

```cmake
# Option 1: Add as subdirectory
add_subdirectory(path/to/Grid9/cpp)
target_link_libraries(your_target grid9)

# Option 2: Find installed package
find_package(Grid9 REQUIRED)
target_link_libraries(your_target Grid9::grid9)
```

### Manual Integration

Simply copy the `include/` and `src/` directories to your project and compile the source files.

## Performance

The C++ implementation is optimized for:
- **High throughput**: Millions of operations per second
- **Low latency**: Minimal overhead per operation  
- **Memory efficiency**: No dynamic allocation in hot paths
- **Cache friendly**: Compact data structures

## Dependencies

- **Required**: C++11 compatible compiler
- **Optional**: Google Test for unit tests
- **Build**: CMake 3.12 or later

## Error Handling

The library uses standard C++ exceptions:
- `std::invalid_argument` for invalid inputs
- `std::out_of_range` for coordinate bounds violations

All functions provide strong exception safety guarantees.

## Thread Safety

The Grid9 library is thread-safe for read operations. All static methods can be called concurrently from multiple threads without synchronization.

## Precision

Grid9 provides:
- **Global coverage**: Works everywhere on Earth
- **Uniform precision**: 2.4-3.5m accuracy consistently worldwide
- **Compact format**: Just 9 characters vs 18+ for what3words
- **No variation**: Same precision in all regions

## Other Language Implementations

Grid9 is available in multiple programming languages with identical precision and functionality:

- **[C#](../csharp/)** - Full .NET implementation
- **[Python](../python/)** - Pure Python implementation
- **[Java](../java/)** - Java 8+ implementation
- **[JavaScript](../javascript/)** - Node.js and browser implementation
- **[Rust](../rust/)** - Memory-safe Rust implementation

All implementations produce identical Grid9 codes and maintain the same precision characteristics.

## License

MIT License - Free for commercial and personal use.