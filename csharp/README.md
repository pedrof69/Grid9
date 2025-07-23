# Grid9 C# / .NET Implementation

The C# implementation of Grid9 - a multi-language coordinate compression library featuring 9-character codes with uniform 3-meter precision globally.

## Directory Structure

```
csharp/
├── src/                           # Core library source code
│   ├── UniformPrecisionCoordinateCompressor.cs    # Main algorithm (uniform global precision)
│   ├── MeterBasedCoordinateCompressor.cs          # Alternative meter-based algorithm  
│   └── CoordinateOperations.cs                    # Batch operations and utilities
├── tests/                         # Comprehensive unit tests (227 tests)
├── demo/                          # Demo console application
├── Grid9.csproj                   # Main library project
├── Grid9.sln                      # Solution file
└── README.md                      # This file
```

## Quick Start

### Building
```bash
cd csharp
dotnet build
```

### Running Tests
```bash
dotnet test tests/Tests.csproj -v normal
# Expected: All 227 tests pass in ~60ms
```

### Running Demo
```bash
dotnet run --project demo
```

## Usage Examples

### Basic Encoding/Decoding
```csharp
using OptimalCoordinateCompression;

// Encode coordinates to 9-character Grid9 code
string code = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);
// Result: "Q7KH2BBYF" (New York City)

// Decode back to coordinates
var (lat, lon) = UniformPrecisionCoordinateCompressor.Decode("Q7KH2BBYF");
// Result: (40.712779, -74.005988)

// Human-readable format
string readable = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060, humanReadable: true);
// Result: "Q7K-H2B-BYF"
```

### Precision and Distance
```csharp
// Get actual precision at location
var (latError, lonError, totalError) = UniformPrecisionCoordinateCompressor.GetActualPrecision(40.7128, -74.0060);
Console.WriteLine($"Precision: {totalError:F1}m");

// Calculate distance between two codes
double distance = UniformPrecisionCoordinateCompressor.CalculateDistance("Q7KH2BBYF", "S50MBZX2Y");
Console.WriteLine($"NYC to London: {distance/1000:F0}km");
```

### Batch Operations
```csharp
// High-throughput coordinate processing
var coordinates = new[] {
    (40.7128, -74.0060),  // NYC  
    (51.5074, -0.1278),   // London
    (35.6762, 139.6503)   // Tokyo
};

string[] codes = CoordinateOperations.BatchEncode(coordinates);
```

### Alternative Algorithm
```csharp
// Use meter-based compressor for different precision characteristics
string code = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
var (lat, lon) = MeterBasedCoordinateCompressor.Decode(code);
```

## Performance

- **Encoding**: ~6.4M operations/second
- **Decoding**: ~7.0M operations/second  
- **Memory**: Minimal allocation per operation
- **Tests**: 227 comprehensive unit tests, all passing

## Requirements

- .NET 8.0 or later
- C# 12.0 language features

## Documentation

For comprehensive documentation, examples, and API reference, see the main [README](../README.md).