#!/usr/bin/env node

/**
 * Grid9 JavaScript Demo
 * 
 * Demonstrates the core functionality of the Grid9 coordinate compression library.
 */

const Grid9 = require('../src/index');

console.log('🌍 Grid9 JavaScript Demo - Uniform Global Precision Coordinate Compression\n');

// Demo 1: Basic encoding and decoding
console.log('📍 Demo 1: Basic Encoding and Decoding');
console.log('=====================================');

const coordinates = [
    { name: 'New York City', lat: 40.7128, lon: -74.0060 },
    { name: 'London', lat: 51.5074, lon: -0.1278 },
    { name: 'Tokyo', lat: 35.6762, lon: 139.6503 },
    { name: 'Sydney', lat: -33.8688, lon: 151.2083 }
];

coordinates.forEach(({ name, lat, lon }) => {
    const encoded = Grid9.encode(lat, lon);
    const formatted = Grid9.format(encoded);
    const decoded = Grid9.decode(encoded);
    const precision = Grid9.precision(lat, lon);
    
    console.log(`${name}:`);
    console.log(`  Input:     ${lat.toFixed(4)}, ${lon.toFixed(4)}`);
    console.log(`  Encoded:   ${encoded}`);
    console.log(`  Formatted: ${formatted}`);
    console.log(`  Decoded:   ${decoded.latitude.toFixed(4)}, ${decoded.longitude.toFixed(4)}`);
    console.log(`  Precision: ${precision.totalErrorM.toFixed(1)}m`);
    console.log('');
});

// Demo 2: Distance calculation
console.log('📏 Demo 2: Distance Calculations');
console.log('================================');

const nycCode = Grid9.encode(40.7128, -74.0060);
const londonCode = Grid9.encode(51.5074, -0.1278);
const distance = Grid9.distance(nycCode, londonCode);

console.log(`NYC Code:    ${nycCode}`);
console.log(`London Code: ${londonCode}`);
console.log(`Distance:    ${(distance / 1000).toFixed(0)} kilometers`);
console.log('');

// Demo 3: Batch operations
console.log('🔄 Demo 3: Batch Operations');
console.log('===========================');

const batchCoords = [
    { lat: 0, lon: 0 },          // Equator/Prime Meridian
    { lat: 23.5, lon: 121.5 },   // Taipei
    { lat: -34.6, lon: -58.4 },  // Buenos Aires
];

const encodedBatch = Grid9.batchEncode(batchCoords);
const decodedBatch = Grid9.batchDecode(encodedBatch);

console.log('Batch encoding results:');
encodedBatch.forEach((code, i) => {
    const orig = batchCoords[i];
    const decoded = decodedBatch[i];
    console.log(`  ${orig.lat.toString().padStart(6)}, ${orig.lon.toString().padStart(7)} → ${code} → ${decoded.lat.toFixed(4)}, ${decoded.lon.toFixed(4)}`);
});
console.log('');

// Demo 4: Spatial operations
console.log('🗺️  Demo 4: Spatial Operations');
console.log('==============================');

const centerLat = 40.7128;
const centerLon = -74.0060;
const centerCode = Grid9.encode(centerLat, centerLon);

console.log(`Center: ${centerCode} (NYC)`);
console.log('Neighbors:');
const neighbors = Grid9.neighbors(centerCode);
neighbors.forEach((neighbor, i) => {
    const dist = Grid9.distance(centerCode, neighbor);
    console.log(`  ${i + 1}. ${neighbor} (${dist.toFixed(0)}m away)`);
});
console.log('');

// Demo 5: Nearby search
console.log('🔍 Demo 5: Nearby Search');
console.log('========================');

const nearbyCoords = Grid9.findNearby(centerLat, centerLon, 500, 5); // 500m radius, max 5 results
console.log(`Finding coordinates within 500m of NYC (${centerCode}):`);
nearbyCoords.slice(0, 5).forEach((code, i) => {
    const distance = Grid9.distance(centerCode, code);
    console.log(`  ${i + 1}. ${code} (${distance.toFixed(0)}m away)`);
});
console.log('');

// Demo 6: Precision analysis
console.log('🎯 Demo 6: Precision Analysis');
console.log('=============================');

const testLatitudes = [0, 30, 45, 60, 75];
console.log('Precision by latitude:');
testLatitudes.forEach(lat => {
    const precision = Grid9.precision(lat, 0);
    console.log(`  ${lat.toString().padStart(2)}°: X±${precision.xErrorM.toFixed(1)}m, Y±${precision.yErrorM.toFixed(1)}m, Total±${precision.totalErrorM.toFixed(1)}m`);
});
console.log('');

// Demo 7: Validation and formatting
console.log('✅ Demo 7: Validation and Formatting');
console.log('====================================');

const testCodes = [
    'Q7KH2BBYF',     // Valid
    'Q7K-H2B-BYF',   // Valid formatted
    'INVALID@#',     // Invalid characters
    'SHORT',         // Too short
    'TOOLONGCODE'    // Too long
];

testCodes.forEach(code => {
    const isValid = Grid9.isValid(code);
    const status = isValid ? '✅ Valid' : '❌ Invalid';
    console.log(`  ${code.padEnd(12)} ${status}`);
});
console.log('');

// Demo 8: Coordinate bounds and center
console.log('📦 Demo 8: Bounding Box and Center');
console.log('==================================');

const polygonCoords = [
    { lat: 40.7, lon: -74.1 },
    { lat: 40.8, lon: -74.0 },
    { lat: 40.7, lon: -73.9 },
    { lat: 40.6, lon: -74.0 }
];

const bbox = Grid9.getBoundingBox(polygonCoords);
const center = Grid9.getCenterPoint(polygonCoords);

console.log('Polygon coordinates:');
polygonCoords.forEach((coord, i) => {
    console.log(`  ${i + 1}. ${coord.lat}, ${coord.lon}`);
});
console.log(`Bounding box: ${bbox.minLat},${bbox.minLon} to ${bbox.maxLat},${bbox.maxLon}`);
console.log(`Center point: ${center.lat.toFixed(4)}, ${center.lon.toFixed(4)}`);
console.log('');

console.log('🎉 Demo completed! Grid9 provides uniform 3-meter precision globally.');
console.log('📚 Learn more: https://github.com/pedrof69/Grid9');
console.log('🌐 Try online: https://pedrof69.github.io/Grid9/');