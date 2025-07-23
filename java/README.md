# Grid9 Java Implementation

Java implementation of the Grid9 coordinate compression system, providing uniform 3-meter precision globally with 9-character codes.

## Features

- **Uniform Precision**: 2.4-3.5m accuracy consistently worldwide
- **Compact Codes**: 9-character base32 strings (vs 19+ for what3words)
- **High Performance**: Millions of operations per second
- **Human Readable**: Optional XXX-XXX-XXX formatting with dashes
- **Global Coverage**: Works everywhere on Earth including oceans and poles
- **Java 8+**: Compatible with Java 8 and later versions

## Quick Start

### Maven Dependency

Add this to your `pom.xml`:

```xml
<dependency>
    <groupId>com.grid9</groupId>
    <artifactId>grid9-java</artifactId>
    <version>1.0.0</version>
</dependency>
```

### Basic Usage

```java
import com.grid9.core.UniformPrecisionCoordinateCompressor;

// Grid9 System (9-Character Precision)
String compact = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060); 
// Result: "Q7KH2BBYF" - New York City (3m precision)

// Human-readable format with dashes
String readable = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, true);
// Result: "Q7K-H2B-BYF" - Same precision, better readability

// Both formats decode to identical coordinates
double[] coords1 = UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYF");
double[] coords2 = UniformPrecisionCoordinateCompressor.decode("Q7K-H2B-BYF");
// Both return: [40.712779, -74.005988]

// Get precision information
double[] precision = UniformPrecisionCoordinateCompressor.getActualPrecision(40.7128, -74.0060);
// Result: [2.4m, 1.8m, 3.0m] - [X error, Y error, total error]
```

### Advanced Features

```java
import com.grid9.core.CoordinateOperations;

// Distance calculation
double distance = UniformPrecisionCoordinateCompressor.calculateDistance("Q7KH2BBYF", "S50MBZX2Y");
// Result: 5,570,224m (NYC to London)

// Validation (works with both formats)
boolean isValid1 = UniformPrecisionCoordinateCompressor.isValidEncoding("Q7KH2BBYF");
boolean isValid2 = UniformPrecisionCoordinateCompressor.isValidEncoding("Q7K-H2B-BYF");
// Both return: true

// Format conversion utilities
String formatted = UniformPrecisionCoordinateCompressor.formatForHumans("Q7KH2BBYF");
// Result: "Q7K-H2B-BYF"

String unformatted = UniformPrecisionCoordinateCompressor.removeFormatting("Q7K-H2B-BYF");
// Result: "Q7KH2BBYF"

// Find neighbors
String[] neighbors = UniformPrecisionCoordinateCompressor.getNeighbors("Q7KH2BBYF");
// Returns up to 8 neighboring Grid9 codes

// Batch operations for high throughput
double[][] coordinates = {{40.7128, -74.0060}, {51.5074, -0.1278}};
String[] encoded = CoordinateOperations.batchEncode(coordinates);
double[][] decoded = CoordinateOperations.batchDecode(encoded);

// Find nearby locations within radius
String[] nearby = CoordinateOperations.findNearby(40.7128, -74.0060, 1000.0); // 1km radius
```

## Building from Source

### Prerequisites

- Java 8 or later
- Maven 3.6 or later

### Build Commands

```bash
# Clone the repository
git clone https://github.com/pedrof69/Grid9.git
cd Grid9/java

# Compile and run tests
mvn clean test

# Build JAR
mvn clean package

# Run demo
mvn exec:java -Dexec.mainClass="com.grid9.demo.Grid9Demo"

# Generate Javadoc
mvn javadoc:javadoc
```

### Running Tests

```bash
# Run all tests
mvn test

# Run specific test class
mvn test -Dtest=UniformPrecisionCoordinateCompressorTest

# Run with verbose output
mvn test -Dtest=UniformPrecisionCoordinateCompressorTest -Dmaven.surefire.debug=true
```

## Performance

The Java implementation delivers high-performance coordinate operations:

- **Encoding**: 1M+ operations/second
- **Decoding**: 1M+ operations/second  
- **Memory**: Minimal allocation per operation
- **Thread-safe**: All operations are stateless and thread-safe

## API Reference

### UniformPrecisionCoordinateCompressor

| Method | Description |
|--------|-------------|
| `encode(double lat, double lon)` | Encode coordinates to 9-character Grid9 string |
| `encode(double lat, double lon, boolean humanReadable)` | Encode with optional XXX-XXX-XXX formatting |
| `decode(String encoded)` | Decode Grid9 string to coordinates [lat, lon] |
| `getActualPrecision(double lat, double lon)` | Get precision errors [X, Y, total] in meters |
| `calculateDistance(String code1, String code2)` | Calculate distance between two Grid9 codes |
| `isValidEncoding(String encoded)` | Validate Grid9 code format |
| `formatForHumans(String code)` | Add dashes for readability |
| `removeFormatting(String formatted)` | Remove dashes from formatted code |
| `isFormattedForHumans(String code)` | Check if code has dash formatting |
| `getNeighbors(String encoded)` | Get up to 8 neighboring Grid9 codes |

### CoordinateOperations

| Method | Description |
|--------|-------------|
| `batchEncode(double[][] coordinates)` | Batch encode multiple coordinate pairs |
| `batchDecode(String[] encoded)` | Batch decode multiple Grid9 codes |
| `findNearby(double lat, double lon, double radius)` | Find Grid9 codes within radius |

## Examples

### Real-World Locations

```java
// Major cities with exact Grid9 codes
double[] nyc = {40.7128, -74.0060};
String nycCode = UniformPrecisionCoordinateCompressor.encode(nyc[0], nyc[1]);
// Result: "Q7KH2BBYF"

double[] london = {51.5074, -0.1278};
String londonCode = UniformPrecisionCoordinateCompressor.encode(london[0], london[1]);
// Result: "S50MBZX2Y"

double[] tokyo = {35.6762, 139.6503};
String tokyoCode = UniformPrecisionCoordinateCompressor.encode(tokyo[0], tokyo[1]);
// Result: "PAYMZ39T7"
```

### Error Handling

```java
// Invalid coordinates throw IllegalArgumentException
try {
    String code = UniformPrecisionCoordinateCompressor.encode(91.0, 0.0); // Invalid latitude
} catch (IllegalArgumentException e) {
    System.out.println("Invalid coordinate: " + e.getMessage());
}

// Invalid codes throw IllegalArgumentException
try {
    double[] coords = UniformPrecisionCoordinateCompressor.decode("INVALID!!");
} catch (IllegalArgumentException e) {
    System.out.println("Invalid Grid9 code: " + e.getMessage());
}
```

## License

MIT License - see [LICENSE](../LICENSE) file for details.

## Other Language Implementations

Grid9 is available in multiple programming languages with identical precision and functionality:

- **[C#](../csharp/)** - Full .NET implementation
- **[Python](../python/)** - Pure Python implementation
- **[JavaScript](../javascript/)** - Node.js and browser implementation
- **[C++](../cpp/)** - High-performance C++11 implementation
- **[Rust](../rust/)** - Memory-safe Rust implementation

All implementations produce identical Grid9 codes and maintain the same precision characteristics.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](../CONTRIBUTING.md) for guidelines.

## Support

- **Issues**: [GitHub Issues](https://github.com/pedrof69/Grid9/issues)
- **Documentation**: [Main README](../README.md)
- **Examples**: See `src/main/java/com/grid9/demo/Grid9Demo.java`