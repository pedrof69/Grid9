# Grid9 Python Implementation

This is the Python implementation of Grid9 - a revolutionary coordinate compression system featuring 9-character Grid9 codes with uniform 3-meter precision globally.

## Installation

From the python directory:

```bash
pip install -e .
```

Or install directly:

```bash
pip install grid9
```

## Quick Start

```python
from grid9 import UniformPrecisionCoordinateCompressor

# Encode coordinates to 9-character Grid9 code
encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060)
print(encoded)  # "Q7KH2BBYF"

# Human-readable format
readable = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, human_readable=True)
print(readable)  # "Q7K-H2B-BYF"

# Decode back to coordinates
lat, lon = UniformPrecisionCoordinateCompressor.decode("Q7KH2BBYF")
print(f"Latitude: {lat}, Longitude: {lon}")  # Close to original values

# Get precision information
x_error, y_error, total_error = UniformPrecisionCoordinateCompressor.get_actual_precision(40.7128, -74.0060)
print(f"Precision: {total_error:.1f} meters")
```

## Available Compressors

### UniformPrecisionCoordinateCompressor
- Optimized for consistent sub-4-meter precision globally
- Simple quantization algorithm with predictable behavior
- Best for applications requiring uniform global precision

### MeterBasedCoordinateCompressor
- Hybrid approach with meter-based latitude quantization
- Longitude precision varies naturally with latitude
- Alternative implementation for specific use cases

## Batch Operations

```python
from grid9 import CoordinateOperations

# Batch encode multiple coordinates
coordinates = [
    (40.7128, -74.0060),   # NYC
    (51.5074, -0.1278),    # London
    (35.6762, 139.6503),   # Tokyo
]
encoded_list = CoordinateOperations.batch_encode(coordinates)

# Find nearby coordinates
nearby = CoordinateOperations.find_nearby(40.7128, -74.0060, radius_meters=1000)
```

## Running Tests

```bash
# Install test dependencies
pip install -e .[dev]

# Run all tests
pytest

# Run with coverage
pytest --cov=grid9
```

## Performance

The Python implementation maintains high performance with:
- Efficient bit manipulation using native Python integers
- Pre-computed lookup tables for base32 encoding/decoding
- Optimized mathematical operations

## Compatibility

- Python 3.7+
- No external dependencies for core functionality
- Optional dev dependencies: pytest, pytest-cov, black, flake8

## License

MIT License - see parent LICENSE file