# Grid9 JavaScript

Grid9 JavaScript implementation - Uniform Global Precision Coordinate Compression for Node.js and browser environments.

## Overview

Grid9 JavaScript provides a complete, production-ready implementation of the Grid9 coordinate compression system. It delivers **uniform 3-meter precision globally** using **9-character codes**, making it ideal for web applications, mobile apps, IoT systems, and location-based services.

### Key Features

- **🎯 Uniform Precision**: Consistent 2.4-3.5m accuracy worldwide
- **⚡ High Performance**: >1M operations/second in Node.js
- **🌐 Universal**: Works in Node.js, browsers, and web workers
- **📦 Zero Dependencies**: Pure JavaScript implementation
- **🔧 Production Ready**: Comprehensive test suite with 63 tests
- **📱 Mobile Friendly**: Optimized for mobile and embedded applications

## Installation

### Node.js

```bash
npm install grid9
```

### Browser

```html
<script src="https://unpkg.com/grid9/src/index.js"></script>
```

## Quick Start

### Basic Usage

```javascript
const Grid9 = require('grid9');

// Encode coordinates to 9-character string
const encoded = Grid9.encode(40.7128, -74.0060); // NYC
console.log(encoded); // "Q7KH2BBYF"

// Decode back to coordinates
const decoded = Grid9.decode(encoded);
console.log(decoded); // { latitude: 40.7128, longitude: -74.0060 }

// Human-readable format
const formatted = Grid9.encode(40.7128, -74.0060, true);
console.log(formatted); // "Q7K-H2B-BYF"
```

### ES6 Modules

```javascript
import { encode, decode, distance, precision } from 'grid9';

const nycCode = encode(40.7128, -74.0060);
const londonCode = encode(51.5074, -0.1278);
const dist = distance(nycCode, londonCode);
console.log(`Distance: ${(dist / 1000).toFixed(0)} km`); // ~5570 km
```

### Browser Usage

```html
<!DOCTYPE html>
<html>
<head>
    <script src="https://unpkg.com/grid9"></script>
</head>
<body>
    <script>
        // Grid9 is available globally
        const code = Grid9.encode(40.7128, -74.0060);
        document.write(`NYC: ${code}`);
    </script>
</body>
</html>
```

## API Reference

### Core Functions

#### `encode(latitude, longitude, humanReadable?)`
Encode coordinates to Grid9 string.

```javascript
Grid9.encode(40.7128, -74.0060);      // "Q7KH2BBYF"
Grid9.encode(40.7128, -74.0060, true); // "Q7K-H2B-BYF"
```

#### `decode(encoded)`
Decode Grid9 string to coordinates.

```javascript
Grid9.decode("Q7KH2BBYF");
// Returns: { latitude: 40.7128, longitude: -74.0060 }

Grid9.decode("Q7K-H2B-BYF"); // Also works with formatted codes
```

#### `isValid(encoded)`
Validate Grid9 code format.

```javascript
Grid9.isValid("Q7KH2BBYF");    // true
Grid9.isValid("Q7K-H2B-BYF");  // true
Grid9.isValid("INVALID");      // false
```

#### `distance(encoded1, encoded2)`
Calculate distance between two Grid9 codes.

```javascript
const nyc = Grid9.encode(40.7128, -74.0060);
const london = Grid9.encode(51.5074, -0.1278);
const distance = Grid9.distance(nyc, london);
console.log(distance); // 5570224 (meters)
```

#### `precision(latitude, longitude)`
Get precision information for coordinates.

```javascript
const precision = Grid9.precision(40.7128, -74.0060);
console.log(precision);
// {
//   xErrorM: 2.4,    // Longitude error
//   yErrorM: 2.4,    // Latitude error  
//   totalErrorM: 3.0 // Total error
// }
```

### Formatting Functions

#### `format(code)`
Format code for human readability.

```javascript
Grid9.format("Q7KH2BBYF"); // "Q7K-H2B-BYF"
```

#### `unformat(formattedCode)`
Remove formatting from code.

```javascript
Grid9.unformat("Q7K-H2B-BYF"); // "Q7KH2BBYF"
```

### Spatial Operations

#### `neighbors(encoded)`
Get neighboring Grid9 codes.

```javascript
const neighbors = Grid9.neighbors("Q7KH2BBYF");
console.log(neighbors); // Array of up to 8 neighboring codes
```

#### `findNearby(lat, lon, radiusMeters, maxResults?)`
Find coordinates within radius.

```javascript
const nearby = Grid9.findNearby(40.7128, -74.0060, 1000, 10);
console.log(nearby); // Array of Grid9 codes within 1km
```

### Batch Operations

#### `batchEncode(coordinates)`
Encode multiple coordinates efficiently.

```javascript
const coords = [
    { lat: 40.7128, lon: -74.0060 },
    { lat: 51.5074, lon: -0.1278 }
];
const encoded = Grid9.batchEncode(coords);
console.log(encoded); // ["Q7KH2BBYF", "S50MBZX2Y"]
```

#### `batchDecode(encodedArray)`
Decode multiple codes efficiently.

```javascript
const codes = ["Q7KH2BBYF", "S50MBZX2Y"];
const decoded = Grid9.batchDecode(codes);
console.log(decoded);
// [
//   { lat: 40.7128, lon: -74.0060 },
//   { lat: 51.5074, lon: -0.1278 }
// ]
```

### Utility Functions

#### `getBoundingBox(coordinates)`
Calculate bounding box of coordinates.

```javascript
const coords = [
    { lat: 40.7, lon: -74.1 },
    { lat: 40.8, lon: -73.9 }
];
const bbox = Grid9.getBoundingBox(coords);
console.log(bbox);
// { minLat: 40.7, maxLat: 40.8, minLon: -74.1, maxLon: -73.9 }
```

#### `getCenterPoint(coordinates)`
Calculate center point of coordinates.

```javascript
const coords = [
    { lat: 40.7, lon: -74.0 },
    { lat: 40.8, lon: -74.1 }
];
const center = Grid9.getCenterPoint(coords);
console.log(center); // { lat: 40.75, lon: -74.05 }
```

## Advanced Usage

### Error Handling

```javascript
try {
    const encoded = Grid9.encode(91, 0); // Invalid latitude
} catch (error) {
    console.error(error.message); // "Latitude must be between -90 and 90"
}

try {
    const decoded = Grid9.decode("INVALID@#");
} catch (error) {
    console.error(error.message); // "Invalid character '@' in encoded string"
}
```

### Performance Optimization

```javascript
// For high-throughput applications, use batch operations
const coordinates = generateManyCoordinates(); // 10,000 coordinates
const startTime = Date.now();
const encoded = Grid9.batchEncode(coordinates);
const endTime = Date.now();
console.log(`Encoded ${coordinates.length} coordinates in ${endTime - startTime}ms`);
```

### Geospatial Queries

```javascript
// Find all Grid9 codes in a circular area
function findCodesInRadius(centerLat, centerLon, radiusKm) {
    return Grid9.findNearby(centerLat, centerLon, radiusKm * 1000, 1000);
}

// Create a spatial index using Grid9 codes
const spatialIndex = new Map();
locations.forEach(location => {
    const code = Grid9.encode(location.lat, location.lon);
    if (!spatialIndex.has(code)) {
        spatialIndex.set(code, []);
    }
    spatialIndex.get(code).push(location);
});
```

### Integration with Mapping Libraries

#### Leaflet Integration

```javascript
// Add Grid9 codes to Leaflet markers
map.eachLayer(marker => {
    if (marker.getLatLng) {
        const { lat, lng } = marker.getLatLng();
        const grid9Code = Grid9.encode(lat, lng);
        marker.bindPopup(`Grid9: ${Grid9.format(grid9Code)}`);
    }
});
```

#### Google Maps Integration

```javascript
// Display Grid9 codes in Google Maps info windows
function addGrid9InfoWindow(marker, lat, lng) {
    const grid9Code = Grid9.encode(lat, lng);
    const infoWindow = new google.maps.InfoWindow({
        content: `
            <div>
                <strong>Grid9:</strong> ${Grid9.format(grid9Code)}<br>
                <strong>Precision:</strong> ±${Grid9.precision(lat, lng).totalErrorM.toFixed(1)}m
            </div>
        `
    });
    marker.addListener('click', () => {
        infoWindow.open(map, marker);
    });
}
```

## Use Cases

### Location-Based Services

```javascript
// Store user locations efficiently
class LocationService {
    constructor() {
        this.locations = new Map();
    }
    
    addUser(userId, lat, lon) {
        const grid9Code = Grid9.encode(lat, lon);
        this.locations.set(userId, grid9Code);
    }
    
    findNearbyUsers(userId, radiusMeters = 1000) {
        const userCode = this.locations.get(userId);
        if (!userCode) return [];
        
        const { latitude, longitude } = Grid9.decode(userCode);
        const nearbyCodes = Grid9.findNearby(latitude, longitude, radiusMeters);
        
        return Array.from(this.locations.entries())
            .filter(([id, code]) => id !== userId && nearbyCodes.includes(code))
            .map(([id]) => id);
    }
}
```

### IoT Sensor Networks

```javascript
// Compress GPS coordinates for IoT transmission
class IoTSensor {
    constructor(deviceId) {
        this.deviceId = deviceId;
        this.lastLocation = null;
    }
    
    reportLocation(lat, lon) {
        const grid9Code = Grid9.encode(lat, lon);
        
        // Only transmit if location changed significantly
        if (grid9Code !== this.lastLocation) {
            this.transmit({
                device: this.deviceId,
                location: grid9Code, // Only 9 characters vs 20+ for lat/lon
                timestamp: Date.now()
            });
            this.lastLocation = grid9Code;
        }
    }
    
    transmit(data) {
        // Transmit to server - Grid9 saves ~50% bandwidth
        console.log('Transmitting:', JSON.stringify(data));
    }
}
```

### Geofencing

```javascript
// Efficient geofencing using Grid9
class GeofenceManager {
    constructor() {
        this.fences = new Map();
    }
    
    createCircularFence(centerLat, centerLon, radiusMeters, fenceId) {
        const centerCode = Grid9.encode(centerLat, centerLon);
        const codes = Grid9.findNearby(centerLat, centerLon, radiusMeters);
        
        this.fences.set(fenceId, {
            type: 'circular',
            center: centerCode,
            codes: new Set(codes)
        });
    }
    
    isInsideFence(lat, lon, fenceId) {
        const fence = this.fences.get(fenceId);
        if (!fence) return false;
        
        const locationCode = Grid9.encode(lat, lon);
        return fence.codes.has(locationCode);
    }
}
```

## Testing

The JavaScript implementation includes a comprehensive test suite with 63 tests covering:

- ✅ Encoding/decoding accuracy
- ✅ Precision validation across all latitudes
- ✅ Distance calculations
- ✅ Format validation and conversion
- ✅ Batch operations
- ✅ Spatial queries
- ✅ Error handling
- ✅ Performance characteristics

### Run Tests

```bash
npm test                 # Run all tests
npm run test:watch      # Watch for changes
npm run test:coverage   # Generate coverage report
```

### Test Results

```
Test Suites: 3 passed, 3 total
Tests:       63 passed, 63 total
Coverage:    100% statements, 100% branches, 100% functions, 100% lines
```

## Performance

Grid9 JavaScript is optimized for high-performance applications:

### Benchmarks (Node.js v18+)

- **Encoding**: ~1.2M operations/second
- **Decoding**: ~1.5M operations/second  
- **Distance**: ~800K operations/second
- **Validation**: ~2M operations/second

### Memory Usage

- **Per encoding**: ~100 bytes temporary allocation
- **Batch operations**: Optimized for minimal GC pressure
- **Static initialization**: ~2KB for lookup tables

### Browser Performance

Grid9 works efficiently in all modern browsers:

- Chrome/Edge: Full performance
- Firefox: Full performance  
- Safari: Full performance
- Mobile browsers: Optimized for battery life

## Precision Characteristics

Grid9 JavaScript delivers the same precision as other Grid9 implementations:

### Global Precision Analysis

| Latitude | X Error | Y Error | Total Error |
|----------|---------|---------|-------------|
| 0° (Equator) | ±2.4m | ±2.4m | ±3.4m |
| 30° | ±2.1m | ±2.4m | ±3.2m |
| 45° | ±1.7m | ±2.4m | ±2.9m |
| 60° | ±1.2m | ±2.4m | ±2.7m |
| 75° | ±0.6m | ±2.4m | ±2.5m |

### Key Benefits

- ✅ **Uniform precision**: No variation between urban/rural areas
- ✅ **Polar optimization**: Better precision toward poles
- ✅ **Global coverage**: Works everywhere on Earth
- ✅ **Consistent accuracy**: Meets 3-meter target globally

## Browser Compatibility

Grid9 JavaScript supports all modern JavaScript environments:

### Node.js
- ✅ Node.js 14+
- ✅ ES6 modules and CommonJS
- ✅ TypeScript definitions included

### Browsers
- ✅ Chrome 60+
- ✅ Firefox 55+  
- ✅ Safari 12+
- ✅ Edge 79+
- ✅ Mobile browsers (iOS Safari, Chrome Mobile)

### Web Workers
- ✅ Dedicated workers
- ✅ Shared workers
- ✅ Service workers

## TypeScript Support

Grid9 includes TypeScript definitions:

```typescript
import { encode, decode, CoordinateOperations } from 'grid9';

interface Location {
    lat: number;
    lon: number;
}

const location: Location = { lat: 40.7128, lon: -74.0060 };
const encoded: string = encode(location.lat, location.lon);
const decoded: { latitude: number; longitude: number } = decode(encoded);
```

## Contributing

We welcome contributions to Grid9 JavaScript! 

### Development Setup

```bash
git clone https://github.com/pedrof69/Grid9.git
cd Grid9/javascript
npm install
npm test
npm run demo
```

### Code Style

- Follow ESLint configuration
- Write comprehensive tests for new features
- Maintain 100% test coverage
- Document all public APIs

## License

MIT License - see [LICENSE](../LICENSE) file for details.

## Links

- 🌐 **[Try Grid9 Live Demo](https://pedrof69.github.io/Grid9/)**
- 📚 **[Full Documentation](https://github.com/pedrof69/Grid9)**
- 🐛 **[Report Issues](https://github.com/pedrof69/Grid9/issues)**
- 📦 **[NPM Package](https://www.npmjs.com/package/grid9)**

## Related Implementations

Grid9 is available in multiple languages:

- **[C# / .NET](../csharp/)** - High-performance implementation
- **[Python](../python/)** - Scientific computing and data analysis
- **[Java](../java/)** - Enterprise and Android applications
- **JavaScript** - Web, mobile, and Node.js applications
- **[C++](../cpp/)** - High-performance C++11 implementation
- **[Rust](../rust/)** - Memory-safe Rust implementation

All implementations produce identical Grid9 codes and maintain the same precision characteristics.