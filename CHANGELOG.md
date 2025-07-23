# Grid9 Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.0.0] - 2025-07-22

### Added
- **UniformPrecisionCoordinateCompressor**: New algorithm achieving uniform 2.4-3.5m precision globally
- `GetNeighbors()` method for spatial queries (returns up to 8 neighboring coordinates)
- `GetActualPrecision()` method for precision measurement at specific locations
- Professional website with interactive demo at https://pedrof69.github.io/Grid9/
- Comprehensive API documentation with verified examples
- Modern logo design with 9-square grid pattern
- GitHub Pages deployment workflow
- 227 comprehensive unit tests covering all functionality

### Changed
- **BREAKING**: Replaced MeterBasedCoordinateCompressor with UniformPrecisionCoordinateCompressor
- **MAJOR**: Eliminated precision variation between cities and rural areas
- **MAJOR**: Global coverage including oceans and polar regions with consistent accuracy
- Updated all documentation examples to match actual implementation behavior
- CoordinateOperations now uses UniformPrecisionCoordinateCompressor consistently

### Fixed
- Corrected Tokyo coordinate example: 35.6762, 139.6503 → PAYMZ39T7 (was M3GK8WQPX)
- Fixed NYC to London distance: 5,570,224m (was ~5,500,000m)
- Updated precision measurements to match actual runtime values
- Removed references to non-existent PerformanceBenchmark class
- Updated all documentation to reflect uniform precision characteristics
- Fixed README examples with actual Grid9 codes
- Improved API documentation with correct performance metrics

### Fixed
- Resolved precision variations that affected different geographic regions
- Fixed edge cases at polar regions and international date line
- All 227 unit tests now passing with improved precision

## [1.0.0] - 2025-01-21

### Added
- Initial release of Grid9 coordinate compression system
- Core compression algorithm achieving what3words precision in 9 characters vs 19+
- 3-meter precision on land areas with hybrid quantization algorithm
- Human-readable formatting with optional dashes (XXX-XXX-XXX format)
- Optimized for automated vehicles, drones, and high-precision applications
- Human-readable Base32 alphabet (excludes I,L,O,U for clarity)
- Batch processing operations for high-throughput scenarios
- Comprehensive test suite with 300+ tests
- Interactive demo application
- Land-focused optimization for maximum terrestrial precision

### Features
- `UniformPrecisionCoordinateCompressor.Encode()` - Compress coordinates to 9 characters
- `UniformPrecisionCoordinateCompressor.Encode(humanReadable)` - Optional dashed formatting
- `UniformPrecisionCoordinateCompressor.Decode()` - Decompress both formats to precise coordinates
- `UniformPrecisionCoordinateCompressor.CalculateDistance()` - Haversine distance calculation
- `UniformPrecisionCoordinateCompressor.GetNeighbors()` - Find adjacent coordinates
- `UniformPrecisionCoordinateCompressor.IsValidEncoding()` - Validate encoded strings (both formats)
- `UniformPrecisionCoordinateCompressor.FormatForHumans()` - Add dashes for readability
- `UniformPrecisionCoordinateCompressor.RemoveFormatting()` - Strip dashes from formatted codes
- `UniformPrecisionCoordinateCompressor.IsFormattedForHumans()` - Check format type
- `CoordinateOperations.BatchEncode()` - High-performance batch encoding
- `CoordinateOperations.BatchDecode()` - High-performance batch decoding
- `CoordinateOperations.FindNearby()` - Radius-based coordinate search

### Performance
- 6.4M+ encoding operations per second
- 7.0M+ decoding operations per second
- 32 bytes per operation memory usage
- ~2.6m average precision on land areas
- Optimized bit allocation (22 bits latitude, 23 bits longitude)

### Documentation
- Comprehensive README with examples
- XML documentation for all public APIs
- Interactive demo with real-world landmarks
- Contributing guidelines
- Mathematical foundation explanations
- API reference documentation

### Technical Details
- Targets .NET 8.0 and later
- Uses aggressive inlining for performance
- Implements hybrid quantization algorithm
- Custom Base32 alphabet for human readability
- Meter-based latitude + degree-based longitude
- Land coverage optimization
- Case-insensitive decoding support
- MIT License (open source)
