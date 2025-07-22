# Grid9 v1.0.0 - Production Release

Grid9 coordinate compression with **uniform 3-meter precision globally**

## 🚀 Key Features

- **Uniform precision**: 2.4-3.5m accuracy everywhere on Earth (cities, rural, oceans, poles)
- **Compact codes**: 9-character Grid9 codes (e.g., `Q7KH2BBYF` for NYC)  
- **Human-readable**: Optional formatting (`Q7K-H2B-BYF`)
- **Global coverage**: Works everywhere including oceans and polar regions
- **High performance**: 6M+ operations/second
- **Zero dependencies**: Pure coordinate math
- **Production ready**: 227 comprehensive tests

## 📝 API Reference

**Main Class: UniformPrecisionCoordinateCompressor**
- `Encode(lat, lon)` → 9-character Grid9 code
- `Encode(lat, lon, humanReadable)` → with optional dashes
- `Decode(code)` → (latitude, longitude)
- `GetNeighbors(code)` → array of up to 8 neighboring codes
- `GetActualPrecision(lat, lon)` → precision measurement
- `CalculateDistance(code1, code2)` → distance in meters
- `IsValidEncoding(code)` → validation

**Batch Operations: CoordinateOperations**
- `BatchEncode(coordinates[])` → high-throughput encoding
- `BatchDecode(codes[])` → high-throughput decoding  
- `FindNearby(lat, lon, radius)` → spatial queries

## 🌍 Example Conversions

```csharp
// New York City
UniformPrecisionCoordinateCompressor.Encode(40.7128, -74.0060)
// → "Q7KH2BBYF" (3.0m precision)

// London  
UniformPrecisionCoordinateCompressor.Encode(51.5074, -0.1278)
// → "S50MBZX2Y" (2.8m precision)

// Tokyo
UniformPrecisionCoordinateCompressor.Encode(35.6762, 139.6503) 
// → "PAYMZ39T7" (3.1m precision)
```

## 🔗 Links

- **Live Demo**: https://pedrof69.github.io/Grid9/
- **GitHub**: https://github.com/pedrof69/Grid9
- **Documentation**: [API Reference](docs/API.md) | [Mathematics](docs/MATHEMATICS.md)
- **Package**: `dotnet add package Grid9`

## 📄 License

MIT License - Free and open source for any use, including commercial applications.

## 🎯 Use Cases

Perfect for:
- Autonomous vehicle waypoint storage
- Precision agriculture coordinate systems
- Drone operation location encoding
- IoT sensor location transmission
- Emergency response coordinate sharing
- Any system requiring compact, accurate location encoding

---

**53% shorter than what3words with the same precision!**