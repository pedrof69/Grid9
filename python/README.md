# Grid9 Python Implementation

The Python implementation of Grid9 - a multi-language coordinate compression library featuring 9-character codes with uniform 3-meter precision globally.

## Directory Structure

```
python/
├── src/                           # Core package source code
│   ├── __init__.py                # Package initialization and exports
│   ├── uniform_precision_compressor.py    # Main algorithm (uniform global precision)
│   ├── meter_based_compressor.py          # Alternative meter-based algorithm
│   └── coordinate_operations.py           # Batch operations and utilities
├── test/                          # Comprehensive unit tests (25 tests)
├── setup.py                       # Package installation configuration
├── test_implementation.py         # Quick verification script
└── README.md                      # This file
```

## Installation

### From Source (Development)
```bash
cd python
pip install -e .
```

### From PyPI (When Published)
```bash
pip install grid9
```

## Quick Start

### Basic Usage
```python
# Note: Import path depends on installation method
# If installed via pip: from grid9 import UniformPrecisionCoordinateCompressor
# From source: from src import UniformPrecisionCoordinateCompressor

from src import UniformPrecisionCoordinateCompressor

# Encode coordinates to 9-character Grid9 code
encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060)
print(encoded)  # "Q7KH2BBYF" (New York City)

# Human-readable format with dashes
readable = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, human_readable=True)
print(readable)  # "Q7K-H2B-BYF"

# Decode back to coordinates
lat, lon = UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYF")
print(f"Coordinates: {lat:.6f}, {lon:.6f}")  # (40.712779, -74.005988)

# Get precision information
x_error, y_error, total_error = UniformPrecisionCoordinateCompressor.get_actual_precision(40.7128, -74.0060)
print(f"Precision: {total_error:.1f} meters")  # ~3.0 meters
```

### Distance and Validation
```python
# Calculate distance between two Grid9 codes
distance = UniformPrecisionCoordinateCompressor.calculate_distance("Q7KH2BBYF", "S50MBZX2Y")
print(f"NYC to London: {distance/1000:.0f}km")  # ~5570km

# Validate Grid9 codes
is_valid = UniformPrecisionCoordinateCompressor.is_valid_encoding("Q7KH2BBYF")
print(f"Valid code: {is_valid}")  # True

# Get neighboring codes for spatial queries
neighbors = UniformPrecisionCoordinateCompressor.get_neighbors("Q7KH2BBYF")
print(f"Found {len(neighbors)} neighboring codes")
```

## Available Algorithms

### UniformPrecisionCoordinateCompressor
- **Best for**: Applications requiring consistent global precision
- **Precision**: 2.4-3.5m uniformly worldwide  
- **Algorithm**: Direct coordinate quantization
- **Use case**: Most applications, mapping, navigation

### MeterBasedCoordinateCompressor  
- **Best for**: Applications where latitude precision is critical
- **Precision**: ~2.4m latitude (constant), longitude varies by location
- **Algorithm**: Meter-based latitude, degree-based longitude quantization
- **Use case**: Specialized applications with specific precision requirements

## Batch Operations

```python
from src import CoordinateOperations

# High-throughput batch encoding
coordinates = [
    (40.7128, -74.0060),   # NYC
    (51.5074, -0.1278),    # London  
    (35.6762, 139.6503),   # Tokyo
]
encoded_list = CoordinateOperations.batch_encode(coordinates)
decoded_list = CoordinateOperations.batch_decode(encoded_list)

# Find coordinates within radius
nearby = CoordinateOperations.find_nearby(
    center_lat=40.7128, 
    center_lon=-74.0060, 
    radius_meters=1000,
    max_results=50
)
print(f"Found {len(nearby)} codes within 1km")
```

## Testing

### Run All Tests
```bash
# Install pytest (if not already installed)
pip install pytest pytest-cov

# Run all 25 tests
python -m pytest test/ -v

# Run with coverage report
python -m pytest test/ --cov=src --cov-report=term-missing
```

### Quick Verification
```bash
# Run implementation test script
python test_implementation.py
```

**Expected Results:**
- 25 tests pass with 92% code coverage
- All algorithms produce identical results to C# implementation
- Performance verification for batch operations

## Performance

The Python implementation delivers:
- **High Performance**: Efficient bit manipulation and lookup tables
- **Memory Efficient**: Minimal allocation per operation
- **Scalable**: Handles batch operations efficiently
- **Compatible**: Produces identical results to C# implementation

## Requirements

- **Python**: 3.7 or later
- **Dependencies**: None for core functionality
- **Dev Dependencies**: pytest, pytest-cov (for testing)

## API Compatibility

The Python API follows Python naming conventions while maintaining functional compatibility with the C# version:

| C# Method | Python Method | Description |
|-----------|---------------|-------------|
| `Encode()` | `encode()` | Encode coordinates to Grid9 code |
| `Decode()` | `decode()` | Decode Grid9 code to coordinates |
| `GetActualPrecision()` | `get_actual_precision()` | Get precision at location |
| `CalculateDistance()` | `calculate_distance()` | Distance between two codes |
| `IsValidEncoding()` | `is_valid_encoding()` | Validate Grid9 code |

## Documentation

For comprehensive documentation, mathematical foundations, and detailed examples, see the main [README](../README.md).