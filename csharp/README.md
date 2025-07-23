# Grid9 C# Implementation

This directory contains the C# implementation of Grid9 - a revolutionary coordinate compression system.

## Structure

- `src/` - Core library source code
  - `UniformPrecisionCoordinateCompressor.cs` - Main compression algorithm
  - `MeterBasedCoordinateCompressor.cs` - Alternative meter-based implementation
  - `CoordinateOperations.cs` - Batch operations and utilities
- `tests/` - Unit tests
- `demo/` - Demo application

## Building

From this directory:

```bash
dotnet build
```

## Running Tests

```bash
dotnet test tests/Tests.csproj
```

## Usage

See the main [README](../README.md) for usage examples and documentation.