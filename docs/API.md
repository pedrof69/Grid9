# Grid9 API Reference

## MeterBasedCoordinateCompressor

The main class for Grid9 coordinate compression and decompression operations with 3-meter precision on land.

### Methods

#### Encode(double latitude, double longitude)

Compresses coordinates to a 9-character Grid9 string with 3-meter precision.

**Parameters:**
- `latitude` (double): Latitude in decimal degrees (-90 to 90)
- `longitude` (double): Longitude in decimal degrees (-180 to 180)

**Returns:**
- `string`: 9-character encoded Grid9 coordinate string

**Throws:**
- `ArgumentOutOfRangeException`: When coordinates are outside valid bounds

**Example:**
```csharp
string encoded = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
// Result: "Q7KH2BBYF"
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
string compact = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060, false);
// Result: "Q7KH2BBYF"

string readable = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060, true);
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
var (lat1, lon1) = MeterBasedCoordinateCompressor.Decode("Q7KH2BBYF");
// Result: (40.712800, -74.006000)

var (lat2, lon2) = MeterBasedCoordinateCompressor.Decode("Q7K-H2B-BYF");
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
double distance = MeterBasedCoordinateCompressor.CalculateDistance("DR5R4Z3M", "DR5R4Z3N");
// Result: ~3.0 (meters)
```

#### GetNeighbors(string encoded)

Generates all adjacent coordinate codes (up to 8 neighbors) for spatial queries.

**Parameters:**
- `encoded` (string): Center coordinate encoding

**Returns:**
- `string[]`: Array of neighboring coordinate encodings

**Example:**
```csharp
string[] neighbors = MeterBasedCoordinateCompressor.GetNeighbors("DR5R4Z3M");
// Result: ["DR5R4Z3N", "DR5R4Z3P", ...] (up to 8 neighbors)
```

#### IsValidEncoding(string encoded)

Validates that a coordinate string is properly formatted. Supports both compact and formatted inputs.

**Parameters:**
- `encoded` (string): String to validate

**Returns:**
- `bool`: True if valid, false otherwise

**Example:**
```csharp
bool isValid1 = MeterBasedCoordinateCompressor.IsValidEncoding("Q7KH2BBYF");
// Result: true

bool isValid2 = MeterBasedCoordinateCompressor.IsValidEncoding("Q7K-H2B-BYF");
// Result: true

bool isValid3 = MeterBasedCoordinateCompressor.IsValidEncoding("INVALID");
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
string formatted = MeterBasedCoordinateCompressor.FormatForHumans("Q7KH2BBYF");
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
string unformatted = MeterBasedCoordinateCompressor.RemoveFormatting("Q7K-H2B-BYF");
// Result: "Q7KH2BBYF"

string unchanged = MeterBasedCoordinateCompressor.RemoveFormatting("Q7KH2BBYF");
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
bool isFormatted1 = MeterBasedCoordinateCompressor.IsFormattedForHumans("Q7K-H2B-BYF");
// Result: true

bool isFormatted2 = MeterBasedCoordinateCompressor.IsFormattedForHumans("Q7KH2BBYF");
// Result: false
```

## CoordinateOperations

High-performance operations for batch processing and spatial queries.

### Methods

#### BatchEncode(ReadOnlySpan<(double lat, double lon)> coordinates)

Batch encodes multiple coordinate pairs for high-throughput scenarios.

**Parameters:**
- `coordinates` (ReadOnlySpan): Span of coordinate tuples

**Returns:**
- `string[]`: Array of encoded coordinate strings

**Example:**
```csharp
var coords = new[] { (40.7128, -74.0060), (51.5074, -0.1278) };
string[] encoded = CoordinateOperations.BatchEncode(coords);
```

#### BatchDecode(ReadOnlySpan<string> encoded)

Batch decodes multiple encoded strings for high-throughput scenarios.

**Parameters:**
- `encoded` (ReadOnlySpan): Span of encoded coordinate strings

**Returns:**
- `(double lat, double lon)[]`: Array of coordinate tuples

**Example:**
```csharp
var encoded = new[] { "DR5R4Z3M", "GCR32JP8" };
var decoded = CoordinateOperations.BatchDecode(encoded);
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

## PerformanceBenchmark

Utilities for benchmarking compression system performance.

### Methods

#### RunBenchmark(int iterations = 1000000)

Benchmarks encoding/decoding performance with random coordinates.

**Parameters:**
- `iterations` (int): Number of operations to benchmark (default: 1,000,000)

**Example:**
```csharp
PerformanceBenchmark.RunBenchmark(100000);
// Outputs performance metrics to console
```

## Constants and Limits

### Coordinate Bounds
- **Latitude**: -90.0 to 90.0 degrees
- **Longitude**: -180.0 to 180.0 degrees

### Precision
- **Target precision**: 3 meters globally
- **Average error**: <1 meter
- **Encoding length**: Exactly 9 characters

### Performance
- **Encoding rate**: 2.2M+ operations/second
- **Decoding rate**: 2.6M+ operations/second
- **Memory**: Zero allocations for core operations

### Alphabet
- **Character set**: `23456789ABCDEFGHJKMNPQRSTUVWXYZ`
- **Excluded characters**: `0`, `1`, `I`, `L`, `O` (prevents transcription errors)
- **Case sensitivity**: Case-insensitive input, uppercase output

## Error Handling

### Common Exceptions

#### ArgumentOutOfRangeException
Thrown when coordinates are outside valid bounds:
```csharp
// Throws ArgumentOutOfRangeException
MeterBasedCoordinateCompressor.Encode(91.0, 0.0); // Latitude > 90
MeterBasedCoordinateCompressor.Encode(0.0, 181.0); // Longitude > 180
```

#### ArgumentException
Thrown when encoded strings are invalid:
```csharp
// Throws ArgumentException
MeterBasedCoordinateCompressor.Decode("ABC");      // Too short
MeterBasedCoordinateCompressor.Decode("DR5R4Z0M"); // Invalid character '0'
MeterBasedCoordinateCompressor.Decode(null);       // Null input
```

## Usage Patterns

### Basic Usage
```csharp
// Simple encoding/decoding
string code = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
var (lat, lon) = MeterBasedCoordinateCompressor.Decode(code);
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
string center = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
string[] neighbors = MeterBasedCoordinateCompressor.GetNeighbors(center);
string[] nearby = CoordinateOperations.FindNearby(40.7128, -74.0060, 500);
```

### Distance Calculations
```csharp
// Calculate distances between encoded points
string nyc = MeterBasedCoordinateCompressor.Encode(40.7128, -74.0060);
string london = MeterBasedCoordinateCompressor.Encode(51.5074, -0.1278);
double distance = MeterBasedCoordinateCompressor.CalculateDistance(nyc, london);
```

### Validation
```csharp
// Validate user input
if (MeterBasedCoordinateCompressor.IsValidEncoding(userInput))
{
    var (lat, lon) = MeterBasedCoordinateCompressor.Decode(userInput);
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
