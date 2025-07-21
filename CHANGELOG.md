# Grid9 Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Professional website with interactive demo
- Marketing materials for Reddit and Hacker News launch
- Modern logo design with 9-square grid pattern
- GitHub Pages deployment workflow

### Changed
- Updated all documentation to reflect Grid9 branding
- Fixed README examples with actual Grid9 codes
- Improved API documentation with correct performance metrics

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
- `MeterBasedCoordinateCompressor.Encode()` - Compress coordinates to 9 characters
- `MeterBasedCoordinateCompressor.Encode(humanReadable)` - Optional dashed formatting
- `MeterBasedCoordinateCompressor.Decode()` - Decompress both formats to precise coordinates
- `MeterBasedCoordinateCompressor.CalculateDistance()` - Haversine distance calculation
- `MeterBasedCoordinateCompressor.GetNeighbors()` - Find adjacent coordinates
- `MeterBasedCoordinateCompressor.IsValidEncoding()` - Validate encoded strings (both formats)
- `MeterBasedCoordinateCompressor.FormatForHumans()` - Add dashes for readability
- `MeterBasedCoordinateCompressor.RemoveFormatting()` - Strip dashes from formatted codes
- `MeterBasedCoordinateCompressor.IsFormattedForHumans()` - Check format type
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
- Grid9 Non-Commercial License (commercial licensing available)
