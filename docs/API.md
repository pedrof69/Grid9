# Grid9 API Reference

Grid9 is available in multiple programming languages with consistent APIs. This document covers the core functionality available across all implementations.

> **Language-Specific Documentation:**
> - [C# Documentation](../csharp/README.md)
> - [Python Documentation](../python/README.md) 
> - [Java Documentation](../java/README.md)
> - [JavaScript Documentation](../javascript/README.md)
> - [C++ Documentation](../cpp/README.md)
> - [Rust Documentation](../rust/README.md)

## Core Functions

The main functions for Grid9 coordinate compression and decompression operations with uniform 3-meter precision globally.

> **Note**: API syntax varies by language. Examples show multiple language variants where helpful.

### Methods

#### encode(latitude, longitude) / Encode(latitude, longitude)

Compresses coordinates to a 9-character Grid9 string with 3-meter precision.

**Parameters:**
- `latitude` (double/float): Latitude in decimal degrees (-90 to 90)
- `longitude` (double/float): Longitude in decimal degrees (-180 to 180)

**Returns:**
- 9-character encoded Grid9 coordinate string

**Errors:**
- Throws exception when coordinates are outside valid bounds

**Examples:**
```csharp
// C#
string encoded = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);

// Python
encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060)

// Java
String encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);

// JavaScript
const encoded = Grid9.encode(40.7128, -74.0060);

// C++
std::string encoded = UniformPrecisionCoordinateCompressor::encode(40.7128, -74.0060);

// Rust
let encoded = encode(40.7128, -74.0060, false)?;

// All result: "Q7KH2BBYF"
```

#### Encode(double latitude, double longitude, bool humanReadable)

Compresses coordinates to a Grid9 string with optional human-readable formatting.

**Parameters:**
- `latitude` (double): Latitude in decimal degrees (-90 to 90)
- `longitude` (double): Longitude in decimal degrees (-180 to 180)
- `humanReadable` (bool): If true, formats as XXX-XXX-XXX with dashes

**Returns:**
- `string`: 9-character (compact) or 11-character (formatted) encoded Grid9 string

**Throws:**
- `ArgumentOutOfRangeException`: When coordinates are outside valid bounds

**Example:**
```csharp
string compact = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060, false);
// Result: "Q7KH2BBYF"

string readable = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060, true);
// Result: "Q7K-H2B-BYF"
```

#### Decode(string encoded)

Decompresses a Grid9 string back to precise coordinates. Supports both compact (9-char) and human-readable (11-char) formats.

**Parameters:**
- `encoded` (string): 9-character compact or 11-character formatted Grid9 string

**Returns:**
- `(double latitude, double longitude)`: Decoded coordinates

**Throws:**
- `ArgumentException`: When encoded string is invalid format

**Example:**
```csharp
var (lat1, lon1) = UniformPrecisionCoordinateCompressor.Decode("Q7KH2BBYF");
// Result: (40.712800, -74.006000)

var (lat2, lon2) = UniformPrecisionCoordinateCompressor.Decode("Q7K-H2B-BYF");
// Result: (40.712800, -74.006000) - same as compact format
```

#### CalculateDistance(string encoded1, string encoded2)

Calculates the great circle distance between two encoded coordinates.

**Parameters:**
- `encoded1` (string): First encoded coordinate
- `encoded2` (string): Second encoded coordinate

**Returns:**
- `double`: Distance in meters

**Example:**
```csharp
double distance = UniformPrecisionCoordinateCompressor.CalculateDistance("Q7KH2BBYF", "S50MBZX2Y");
// Result: 5570224 (meters between NYC and London)
```

#### GetActualPrecision(double latitude, double longitude)

Get the actual precision at a given coordinate location.

**Parameters:**
- `latitude` (double): Latitude in decimal degrees
- `longitude` (double): Longitude in decimal degrees

**Returns:**
- `(double xErrorM, double yErrorM, double totalErrorM)`: Tuple of precision errors in meters

**Example:**
```csharp
var (xError, yError, totalError) = UniformPrecisionCoordinateCompressor.GetActualPrecision(40.7128, -74.0060);
// Result: (1.8m, 2.4m, 3.0m)
```

#### GetNeighbors(string encoded)

Generate neighboring coordinates for spatial queries. Returns up to 8 neighboring Grid9 codes around the given coordinate.

**Parameters:**
- `encoded` (string): Center coordinate encoding

**Returns:**
- `string[]`: Array of neighboring coordinate encodings

**Example:**
```csharp
string[] neighbors = UniformPrecisionCoordinateCompressor.GetNeighbors("Q7KH2BBYF");
// Result: Array of up to 8 neighboring Grid9 codes
```

#### IsValidEncoding(string encoded)

Validates that a coordinate string is properly formatted. Supports both compact and formatted inputs.

**Parameters:**
- `encoded` (string): String to validate

**Returns:**
- `bool`: True if valid, false otherwise

**Example:**
```csharp
bool isValid1 = UniformPrecisionCoordinateCompressor.IsValidEncoding("Q7KH2BBYF");
// Result: true

bool isValid2 = UniformPrecisionCoordinateCompressor.IsValidEncoding("Q7K-H2B-BYF");
// Result: true

bool isValid3 = UniformPrecisionCoordinateCompressor.IsValidEncoding("INVALID");
// Result: false
```

#### FormatForHumans(string code)

Format a 9-character Grid9 code for human readability by adding dashes.

**Parameters:**
- `code` (string): 9-character Grid9 code

**Returns:**
- `string`: 11-character formatted code with dashes

**Throws:**
- `ArgumentException`: When input is not exactly 9 characters

**Example:**
```csharp
string formatted = UniformPrecisionCoordinateCompressor.FormatForHumans("Q7KH2BBYF");
// Result: "Q7K-H2B-BYF"
```

#### RemoveFormatting(string formattedCode)

Remove formatting from a human-readable Grid9 code.

**Parameters:**
- `formattedCode` (string): 9 or 11-character Grid9 code

**Returns:**
- `string`: 9-character unformatted code

**Throws:**
- `ArgumentException`: When input format is invalid

**Example:**
```csharp
string unformatted = UniformPrecisionCoordinateCompressor.RemoveFormatting("Q7K-H2B-BYF");
// Result: "Q7KH2BBYF"

string unchanged = UniformPrecisionCoordinateCompressor.RemoveFormatting("Q7KH2BBYF");
// Result: "Q7KH2BBYF" - already unformatted
```

#### IsFormattedForHumans(string code)

Check if a string is in human-readable format with dashes.

**Parameters:**
- `code` (string): Code to check

**Returns:**
- `bool`: True if formatted with dashes, false otherwise

**Example:**
```csharp
bool isFormatted1 = UniformPrecisionCoordinateCompressor.IsFormattedForHumans("Q7K-H2B-BYF");
// Result: true

bool isFormatted2 = UniformPrecisionCoordinateCompressor.IsFormattedForHumans("Q7KH2BBYF");
// Result: false
```

## CoordinateOperations

High-performance operations for batch processing and spatial queries.

### Methods

#### BatchEncode(ReadOnlySpan<(double lat, double lon)> coordinates)

Batch encodes multiple coordinate pairs for high-throughput scenarios using the UniformPrecisionCoordinateCompressor.

**Parameters:**
- `coordinates` (ReadOnlySpan): Span of coordinate tuples

**Returns:**
- `string[]`: Array of encoded Grid9 coordinate strings

**Example:**
```csharp
var coords = new[] { (40.7128, -74.0060), (51.5074, -0.1278) };
string[] encoded = CoordinateOperations.BatchEncode(coords);
// Result: ["Q7KH2BBYF", "S50MBZX2Y"]
```

#### BatchDecode(ReadOnlySpan<string> encoded)

Batch decodes multiple encoded strings for high-throughput scenarios using the UniformPrecisionCoordinateCompressor.

**Parameters:**
- `encoded` (ReadOnlySpan): Span of encoded Grid9 coordinate strings

**Returns:**
- `(double lat, double lon)[]`: Array of coordinate tuples

**Example:**
```csharp
var encoded = new[] { "Q7KH2BBYF", "S50MBZX2Y" };
var decoded = CoordinateOperations.BatchDecode(encoded);
// Result: [(40.7128, -74.0060), (51.5074, -0.1278)]
```

#### FindNearby(double centerLat, double centerLon, double radiusMeters, int maxResults = 100)

Finds all coordinates within a specified radius of a center point.

**Parameters:**
- `centerLat` (double): Center latitude
- `centerLon` (double): Center longitude  
- `radiusMeters` (double): Search radius in meters
- `maxResults` (int): Maximum number of results (default: 100)

**Returns:**
- `string[]`: Array of encoded coordinates within radius

**Example:**
```csharp
string[] nearby = CoordinateOperations.FindNearby(40.7128, -74.0060, 100);
// Returns coordinates within 100 meters of NYC coordinates
```


## Constants and Limits

### Coordinate Bounds
- **Latitude**: -90.0 to 90.0 degrees
- **Longitude**: -180.0 to 180.0 degrees

### Precision
- **Target precision**: 3 meters globally
- **Actual precision**: 2.4m - 3.5m uniform globally
- **Encoding length**: Exactly 9 characters (11 with formatting)
- **Uniformity**: No variation between cities and rural areas

### Performance
- **Encoding rate**: 6.4M+ operations/second
- **Decoding rate**: 7.0M+ operations/second
- **Memory**: 32 bytes per operation

### Alphabet
- **Character set**: `0123456789ABCDEFGHJKMNPQRSTVWXYZ`
- **Excluded characters**: `I`, `L`, `O`, `U` (prevents confusion)
- **Case sensitivity**: Case-insensitive input, uppercase output

## Error Handling

### Common Exceptions

#### ArgumentOutOfRangeException
Thrown when coordinates are outside valid bounds:
```csharp
// Throws ArgumentOutOfRangeException
UniformPrecisionCoordinateCompressor.Encode(91.0, 0.0); // Latitude > 90
UniformPrecisionCoordinateCompressor.Encode(0.0, 181.0); // Longitude > 180
```

#### ArgumentException
Thrown when encoded strings are invalid:
```csharp
// Throws ArgumentException
UniformPrecisionCoordinateCompressor.Decode("ABC");      // Too short
UniformPrecisionCoordinateCompressor.Decode("Q7KH2BBYL"); // Invalid character 'L'
UniformPrecisionCoordinateCompressor.Decode(null);       // Null input
```

## Usage Patterns

### Basic Usage
```csharp
// Simple encoding/decoding
string code = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);
var (lat, lon) = UniformPrecisionCoordinateCompressor.Decode(code);
```

### High-Performance Batch Processing
```csharp
// Process thousands of coordinates efficiently
var coordinates = LoadCoordinatesFromDatabase();
string[] encoded = CoordinateOperations.BatchEncode(coordinates);
var decoded = CoordinateOperations.BatchDecode(encoded);
```

### Spatial Queries
```csharp
// Find nearby locations
string center = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);
string[] neighbors = UniformPrecisionCoordinateCompressor.GetNeighbors(center);
string[] nearby = CoordinateOperations.FindNearby(40.7128, -74.0060, 500);
```

### Distance Calculations
```csharp
// Calculate distances between encoded points
string nyc = UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060);
string london = UniformPrecisionCoordinateCompressor.Encode(51.5074, -0.1278);
double distance = UniformPrecisionCoordinateCompressor.CalculateDistance(nyc, london);
```

### Validation
```csharp
// Validate user input
if (UniformPrecisionCoordinateCompressor.IsValidEncoding(userInput))
{
    var (lat, lon) = UniformPrecisionCoordinateCompressor.Decode(userInput);
    // Process valid coordinates
}
```

## Thread Safety

All methods in this library are **thread-safe** and can be called concurrently from multiple threads without synchronization. The implementation uses immutable data structures and lock-free algorithms for optimal performance in multithreaded scenarios.

## Performance Considerations

### Hot Path Optimization
- Core methods use `AggressiveInlining` for maximum performance
- Lookup tables are pre-computed during static initialization
- No allocations in encoding/decoding operations
- Stack-allocated spans for temporary data

### Memory Usage
- Static memory footprint: ~2KB for lookup tables
- Runtime allocations: Zero for core operations
- String allocation only for final encoded result
- Batch operations reuse arrays when possible

### Optimization Tips
1. Use batch operations for processing multiple coordinates
2. Cache encoded strings when possible to avoid re-encoding
3. Use `ReadOnlySpan<T>` for input data when available
4. Consider object pooling for high-frequency scenarios
