# Grid9 Rust Implementation

High-performance Rust implementation of the Grid9 coordinate compression system with **9-character codes** providing **uniform 3-meter precision globally**.

## Features

- **Zero-allocation encoding/decoding** - Optimized for performance
- **Memory safety** - Leverages Rust's ownership system
- **Comprehensive error handling** - All operations return `Result<T, Grid9Error>`
- **Serde support** - Optional serialization/deserialization (feature flag)
- **No unsafe code** - Pure safe Rust implementation
- **Extensive testing** - Comprehensive test suite with edge cases
- **Benchmarking** - Performance benchmarks with Criterion

## Quick Start

### Installation

Add to your `Cargo.toml`:

```toml
[dependencies]
grid9 = "1.0.0"

# Optional: Enable serde support for serialization
grid9 = { version = "1.0.0", features = ["serde"] }
```

### Basic Usage

```rust
use grid9::{encode, decode, calculate_distance, Result};

fn main() -> Result<()> {
    // Encode coordinates
    let code = encode(40.7128, -74.0060, false)?; // "Q7KH2BBYF" - NYC
    println!("NYC: {}", code);
    
    // Human-readable format
    let readable = encode(40.7128, -74.0060, true)?; // "Q7K-H2B-BYF"
    println!("NYC readable: {}", readable);
    
    // Decode coordinates
    let (lat, lon) = decode(&code)?;
    println!("Decoded: ({:.6}, {:.6})", lat, lon);
    
    // Calculate distance between two codes
    let london_code = encode(51.5074, -0.1278, false)?;
    let distance = calculate_distance(&code, &london_code)?;
    println!("Distance NYC to London: {:.0} meters", distance);
    
    Ok(())
}
```

### Batch Operations

```rust
use grid9::{batch_encode, batch_decode, Coordinate, Result};

fn batch_example() -> Result<()> {
    let coordinates = vec![
        Coordinate::new(40.7128, -74.0060), // NYC
        Coordinate::new(51.5074, -0.1278),  // London
        Coordinate::new(35.6762, 139.6503), // Tokyo
    ];
    
    // Batch encode
    let encoded = batch_encode(&coordinates, false)?;
    println!("Encoded {} coordinates", encoded.len());
    
    // Batch decode
    let decoded = batch_decode(&encoded)?;
    println!("Decoded {} coordinates", decoded.len());
    
    Ok(())
}
```

### Spatial Operations

```rust
use grid9::{find_nearby, get_bounding_box, get_center_point, Coordinate, Result};

fn spatial_example() -> Result<()> {
    let coordinates = vec![
        Coordinate::new(40.7128, -74.0060), // NYC
        Coordinate::new(51.5074, -0.1278),  // London
    ];
    
    // Find nearby points
    let nearby = find_nearby(40.7128, -74.0060, 1000.0, 10)?;
    println!("Found {} nearby points", nearby.len());
    
    // Get bounding box
    let bbox = get_bounding_box(&coordinates)?;
    println!("Bounding box: ({:.6}, {:.6}) to ({:.6}, {:.6})", 
             bbox.min_lat, bbox.min_lon, bbox.max_lat, bbox.max_lon);
    
    // Get center point
    let center = get_center_point(&coordinates)?;
    println!("Center: ({:.6}, {:.6})", center.lat, center.lon);
    
    Ok(())
}
```

## API Reference

### Core Functions

#### `encode(latitude: f64, longitude: f64, human_readable: bool) -> Result<String>`
Encodes coordinates to a Grid9 string.
- Returns 9-character code or XXX-XXX-XXX format if human_readable is true
- **Example**: `encode(40.7128, -74.0060, false)? // "Q7KH2BBYF"`

#### `decode(encoded: &str) -> Result<(f64, f64)>`
Decodes Grid9 string to coordinates.
- Accepts both compact and dash-formatted strings
- **Example**: `decode("Q7KH2BBYF")? // (40.712779, -74.005988)`

#### `calculate_distance(encoded1: &str, encoded2: &str) -> Result<f64>`
Calculates distance between two Grid9 codes in meters.
- Uses Haversine formula for accurate results
- **Example**: `calculate_distance("Q7KH2BBYF", "S50MBZX2Y")? // ~5,570,224m`

#### `is_valid_encoding(encoded: &str) -> bool`
Validates Grid9 encoding format.
- **Example**: `is_valid_encoding("Q7KH2BBYF") // true`

#### `get_actual_precision(latitude: f64, longitude: f64) -> Result<PrecisionInfo>`
Returns precision information for coordinates.
- **Example**: Returns lat/lon/total error in meters

### Batch Operations

#### `batch_encode(coordinates: &[Coordinate], human_readable: bool) -> Result<Vec<String>>`
Batch encodes multiple coordinates for high throughput.

#### `batch_decode(encoded: &[String]) -> Result<Vec<Coordinate>>`
Batch decodes multiple encoded strings.

### Spatial Operations

#### `find_nearby(center_lat: f64, center_lon: f64, radius_meters: f64, max_results: usize) -> Result<Vec<String>>`
Finds Grid9 codes within a radius of a center point.

#### `get_bounding_box(coordinates: &[Coordinate]) -> Result<BoundingBox>`
Calculates bounding box containing all coordinates.

#### `get_center_point(coordinates: &[Coordinate]) -> Result<Coordinate>`
Calculates center point (arithmetic mean) of coordinates.

#### `group_by_grid9(coordinates: &[Coordinate], human_readable: bool) -> Result<HashMap<String, Vec<Coordinate>>>`
Groups coordinates by their Grid9 codes for spatial indexing.

### Utility Functions

#### `format_for_humans(encoded: &str) -> String`
Converts compact format to XXX-XXX-XXX.

#### `remove_formatting(formatted: &str) -> String`
Removes dashes from formatted string.

## Data Types

### `Coordinate`
```rust
pub struct Coordinate {
    pub lat: f64,
    pub lon: f64,
}
```

### `BoundingBox`
```rust
pub struct BoundingBox {
    pub min_lat: f64,
    pub max_lat: f64,
    pub min_lon: f64,
    pub max_lon: f64,
}
```

### `PrecisionInfo`
```rust
pub struct PrecisionInfo {
    pub lat_error_m: f64,
    pub lon_error_m: f64,
    pub total_error_m: f64,
}
```

### `Grid9Error`
```rust
pub enum Grid9Error {
    InvalidLatitude(f64),
    InvalidLongitude(f64),
    InvalidLength(usize),
    InvalidCharacter(char),
    EmptyInput,
}
```

## Building and Testing

### Build
```bash
# Standard build
cargo build

# Release build (optimized)
cargo build --release

# With serde support
cargo build --features serde
```

### Testing
```bash
# Run all tests
cargo test

# Run tests with output
cargo test -- --nocapture

# Run specific test
cargo test test_encode_decode_roundtrip
```

### Benchmarking
```bash
# Run performance benchmarks
cargo bench

# Run specific benchmark
cargo bench encode
```

### Examples
```bash
# Run the demo
cargo run --example demo

# Run with release optimizations
cargo run --release --example demo
```

## Performance

The Rust implementation is optimized for:
- **Zero allocations** in hot paths
- **SIMD-friendly** operations where possible
- **Cache-efficient** data structures
- **Minimal branching** in critical loops

Typical performance:
- **Encoding**: ~10M operations/second
- **Decoding**: ~12M operations/second
- **Distance calculation**: ~8M operations/second

## Features

### Default Features
- Core Grid9 functionality
- Standard error handling
- All spatial operations

### Optional Features

#### `serde`
Enables serialization/deserialization support for all data types:
```toml
[dependencies]
grid9 = { version = "1.0.0", features = ["serde"] }
```

```rust
use serde::{Serialize, Deserialize};
use grid9::Coordinate;

#[derive(Serialize, Deserialize)]
struct Location {
    name: String,
    coord: Coordinate,
}
```

## Error Handling

All fallible operations return `Result<T, Grid9Error>`:

```rust
use grid9::{encode, Grid9Error};

match encode(91.0, 0.0, false) {
    Ok(code) => println!("Encoded: {}", code),
    Err(Grid9Error::InvalidLatitude(lat)) => {
        eprintln!("Invalid latitude: {}", lat);
    }
    Err(e) => eprintln!("Other error: {}", e),
}
```

## Thread Safety

The Grid9 library is fully thread-safe:
- All functions are `Send + Sync`
- No global state or mutexes
- Safe for concurrent use from multiple threads

## Memory Usage

- **Zero allocations** for encode/decode operations
- **Minimal allocations** for batch operations (only for result vectors)
- **Stack-based** data structures where possible
- **Efficient string handling** with pre-allocated buffers

## Precision

Grid9 provides:
- **Global coverage**: Works everywhere on Earth
- **Uniform precision**: 2.4-3.5m accuracy consistently worldwide  
- **Compact format**: Just 9 characters vs 18+ for what3words
- **No variation**: Same precision in all regions

## Integration Examples

### With Actix Web
```rust
use actix_web::{web, App, HttpServer, Result as ActixResult};
use grid9::{encode, decode};

async fn encode_endpoint(coords: web::Json<(f64, f64)>) -> ActixResult<String> {
    let code = encode(coords.0, coords.1, false)
        .map_err(|e| actix_web::error::ErrorBadRequest(e))?;
    Ok(code)
}
```

### With SQLite
```rust
use rusqlite::{params, Connection, Result as SqlResult};
use grid9::{encode, Coordinate};

fn store_location(conn: &Connection, name: &str, coord: &Coordinate) -> SqlResult<()> {
    let code = encode(coord.lat, coord.lon, false).unwrap();
    conn.execute(
        "INSERT INTO locations (name, grid9_code, lat, lon) VALUES (?1, ?2, ?3, ?4)",
        params![name, code, coord.lat, coord.lon],
    )?;
    Ok(())
}
```

### With Tokio
```rust
use tokio;
use grid9::{batch_encode, Coordinate};

#[tokio::main]
async fn main() {
    let coordinates = vec![
        Coordinate::new(40.7128, -74.0060),
        Coordinate::new(51.5074, -0.1278),
    ];
    
    let encoded = tokio::task::spawn_blocking(move || {
        batch_encode(&coordinates, false)
    }).await.unwrap().unwrap();
    
    println!("Encoded {} coordinates", encoded.len());
}
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests for new functionality
4. Run `cargo test` and `cargo clippy`
5. Submit a pull request

## Other Language Implementations

Grid9 is available in multiple programming languages with identical precision and functionality:

- **[C#](../csharp/)** - Full .NET implementation
- **[Python](../python/)** - Pure Python implementation
- **[Java](../java/)** - Java 8+ implementation
- **[JavaScript](../javascript/)** - Node.js and browser implementation
- **[C++](../cpp/)** - High-performance C++11 implementation

All implementations produce identical Grid9 codes and maintain the same precision characteristics.

## License

MIT License - Free for commercial and personal use.

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history.