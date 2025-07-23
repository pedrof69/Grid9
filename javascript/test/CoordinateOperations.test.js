const CoordinateOperations = require('../src/CoordinateOperations');
const UniformPrecisionCoordinateCompressor = require('../src/UniformPrecisionCoordinateCompressor');

describe('CoordinateOperations', () => {
    describe('batchEncode', () => {
        test('encodes multiple coordinates', () => {
            const coordinates = [
                { lat: 40.7128, lon: -74.0060 }, // New York
                { lat: 51.5074, lon: -0.1278 },  // London
                { lat: 35.6762, lon: 139.6503 }  // Tokyo
            ];

            const results = CoordinateOperations.batchEncode(coordinates);

            expect(results).toHaveLength(3);
            results.forEach(encoded => {
                expect(encoded).toHaveLength(9);
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(true);
            });
        });

        test('handles empty array', () => {
            const results = CoordinateOperations.batchEncode([]);
            expect(results).toHaveLength(0);
        });

        test('throws error for invalid input', () => {
            expect(() => {
                CoordinateOperations.batchEncode(null);
            }).toThrow('Coordinates must be an array');

            expect(() => {
                CoordinateOperations.batchEncode([{ lat: 'invalid', lon: 0 }]);
            }).toThrow('lat and lon must be numbers');

            expect(() => {
                CoordinateOperations.batchEncode([{ lat: 40, lon: null }]);
            }).toThrow('lat and lon must be numbers');
        });

        test('maintains order of results', () => {
            const coordinates = [
                { lat: 0, lon: 0 },
                { lat: 10, lon: 10 },
                { lat: 20, lon: 20 }
            ];

            const results = CoordinateOperations.batchEncode(coordinates);
            const individual = coordinates.map(coord => 
                UniformPrecisionCoordinateCompressor.encode(coord.lat, coord.lon)
            );

            expect(results).toEqual(individual);
        });
    });

    describe('batchDecode', () => {
        test('decodes multiple encoded strings', () => {
            const encoded = [
                'Q7KH2BBYF', // NYC approximate
                'S50MBZX2Y', // London approximate
                'PAYMZ39T7'  // Tokyo approximate
            ];

            const results = CoordinateOperations.batchDecode(encoded);

            expect(results).toHaveLength(3);
            results.forEach(coord => {
                expect(typeof coord.lat).toBe('number');
                expect(typeof coord.lon).toBe('number');
                expect(coord.lat).toBeGreaterThanOrEqual(-90);
                expect(coord.lat).toBeLessThanOrEqual(90);
                expect(coord.lon).toBeGreaterThanOrEqual(-180);
                expect(coord.lon).toBeLessThanOrEqual(180);
            });
        });

        test('handles empty array', () => {
            const results = CoordinateOperations.batchDecode([]);
            expect(results).toHaveLength(0);
        });

        test('throws error for invalid input', () => {
            expect(() => {
                CoordinateOperations.batchDecode(null);
            }).toThrow('Encoded must be an array');

            expect(() => {
                CoordinateOperations.batchDecode([123]);
            }).toThrow('must be a string');

            expect(() => {
                CoordinateOperations.batchDecode(['INVALID@#']);
            }).toThrow('Invalid character');
        });

        test('round trip consistency', () => {
            const coordinates = [
                { lat: 40.7128, lon: -74.0060 },
                { lat: 0, lon: 0 },
                { lat: -33.8688, lon: 151.2083 }
            ];

            const encoded = CoordinateOperations.batchEncode(coordinates);
            const decoded = CoordinateOperations.batchDecode(encoded);

            decoded.forEach((coord, i) => {
                expect(Math.abs(coord.lat - coordinates[i].lat)).toBeLessThan(0.01);
                expect(Math.abs(coord.lon - coordinates[i].lon)).toBeLessThan(0.01);
            });
        });
    });

    describe('findNearby', () => {
        test('finds coordinates within radius', () => {
            const centerLat = 40.7128;
            const centerLon = -74.0060;
            const radiusMeters = 1000; // 1km

            const results = CoordinateOperations.findNearby(centerLat, centerLon, radiusMeters, 10);

            expect(results).toBeInstanceOf(Array);
            expect(results.length).toBeGreaterThan(0);
            expect(results.length).toBeLessThanOrEqual(10);

            // All results should be valid encodings
            results.forEach(encoded => {
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(true);
            });
        });

        test('respects maximum results limit', () => {
            const results = CoordinateOperations.findNearby(0, 0, 5000, 5);
            expect(results.length).toBeLessThanOrEqual(5);
        });

        test('all results are within specified radius', () => {
            const centerLat = 0;
            const centerLon = 0;
            const radiusMeters = 1000;
            const centerEncoded = UniformPrecisionCoordinateCompressor.encode(centerLat, centerLon);

            const results = CoordinateOperations.findNearby(centerLat, centerLon, radiusMeters, 20);

            results.forEach(encoded => {
                const distance = UniformPrecisionCoordinateCompressor.calculateDistance(centerEncoded, encoded);
                expect(distance).toBeLessThanOrEqual(radiusMeters + 10); // Allow small tolerance for grid quantization
            });
        });

        test('throws error for invalid parameters', () => {
            expect(() => {
                CoordinateOperations.findNearby('invalid', 0, 1000);
            }).toThrow('Center coordinates must be numbers');

            expect(() => {
                CoordinateOperations.findNearby(0, 0, -100);
            }).toThrow('Radius must be a positive number');

            expect(() => {
                CoordinateOperations.findNearby(0, 0, 1000, -1);
            }).toThrow('Max results must be a positive number');
        });

        test('handles edge cases', () => {
            // Test near poles
            const polarResults = CoordinateOperations.findNearby(85, 0, 1000, 10);
            expect(polarResults).toBeInstanceOf(Array);

            // Test near antimeridian
            const antimeridianResults = CoordinateOperations.findNearby(0, 179, 1000, 10);
            expect(antimeridianResults).toBeInstanceOf(Array);
        });
    });

    describe('getBoundingBox', () => {
        test('calculates correct bounding box', () => {
            const coordinates = [
                { lat: 40.7128, lon: -74.0060 }, // New York
                { lat: 51.5074, lon: -0.1278 },  // London
                { lat: 35.6762, lon: 139.6503 }  // Tokyo
            ];

            const boundingBox = CoordinateOperations.getBoundingBox(coordinates);

            expect(boundingBox.minLat).toBe(35.6762);
            expect(boundingBox.maxLat).toBe(51.5074);
            expect(boundingBox.minLon).toBe(-74.0060);
            expect(boundingBox.maxLon).toBe(139.6503);
        });

        test('handles single coordinate', () => {
            const coordinates = [{ lat: 40.7128, lon: -74.0060 }];
            const boundingBox = CoordinateOperations.getBoundingBox(coordinates);

            expect(boundingBox.minLat).toBe(40.7128);
            expect(boundingBox.maxLat).toBe(40.7128);
            expect(boundingBox.minLon).toBe(-74.0060);
            expect(boundingBox.maxLon).toBe(-74.0060);
        });

        test('throws error for invalid input', () => {
            expect(() => {
                CoordinateOperations.getBoundingBox([]);
            }).toThrow('Coordinates must be a non-empty array');

            expect(() => {
                CoordinateOperations.getBoundingBox(null);
            }).toThrow('Coordinates must be a non-empty array');

            expect(() => {
                CoordinateOperations.getBoundingBox([{ lat: 'invalid', lon: 0 }]);
            }).toThrow('Each coordinate must have numeric lat and lon properties');
        });
    });

    describe('getCenterPoint', () => {
        test('calculates correct center point', () => {
            const coordinates = [
                { lat: 0, lon: 0 },
                { lat: 10, lon: 10 },
                { lat: 20, lon: 20 }
            ];

            const center = CoordinateOperations.getCenterPoint(coordinates);

            expect(center.lat).toBe(10);
            expect(center.lon).toBe(10);
        });

        test('handles single coordinate', () => {
            const coordinates = [{ lat: 40.7128, lon: -74.0060 }];
            const center = CoordinateOperations.getCenterPoint(coordinates);

            expect(center.lat).toBe(40.7128);
            expect(center.lon).toBe(-74.0060);
        });

        test('calculates center of real world coordinates', () => {
            const coordinates = [
                { lat: 40.7128, lon: -74.0060 }, // New York
                { lat: 51.5074, lon: -0.1278 }   // London
            ];

            const center = CoordinateOperations.getCenterPoint(coordinates);

            expect(center.lat).toBeCloseTo(46.1101, 3);
            expect(center.lon).toBeCloseTo(-37.0669, 3);
        });

        test('throws error for invalid input', () => {
            expect(() => {
                CoordinateOperations.getCenterPoint([]);
            }).toThrow('Coordinates must be a non-empty array');

            expect(() => {
                CoordinateOperations.getCenterPoint(null);
            }).toThrow('Coordinates must be a non-empty array');

            expect(() => {
                CoordinateOperations.getCenterPoint([{ lat: 'invalid', lon: 0 }]);
            }).toThrow('Each coordinate must have numeric lat and lon properties');
        });
    });

    describe('integration tests', () => {
        test('batch operations work together', () => {
            const originalCoords = [
                { lat: 40.7128, lon: -74.0060 },
                { lat: 51.5074, lon: -0.1278 },
                { lat: 35.6762, lon: 139.6503 }
            ];

            // Encode batch
            const encoded = CoordinateOperations.batchEncode(originalCoords);
            
            // Decode batch
            const decoded = CoordinateOperations.batchDecode(encoded);

            // Verify round trip
            decoded.forEach((coord, i) => {
                expect(Math.abs(coord.lat - originalCoords[i].lat)).toBeLessThan(0.01);
                expect(Math.abs(coord.lon - originalCoords[i].lon)).toBeLessThan(0.01);
            });
        });

        test('spatial operations consistency', () => {
            const coordinates = [
                { lat: 40.7, lon: -74.0 },
                { lat: 40.8, lon: -74.1 },
                { lat: 40.9, lon: -74.2 }
            ];

            const boundingBox = CoordinateOperations.getBoundingBox(coordinates);
            const center = CoordinateOperations.getCenterPoint(coordinates);

            // Center should be within bounding box
            expect(center.lat).toBeGreaterThanOrEqual(boundingBox.minLat);
            expect(center.lat).toBeLessThanOrEqual(boundingBox.maxLat);
            expect(center.lon).toBeGreaterThanOrEqual(boundingBox.minLon);
            expect(center.lon).toBeLessThanOrEqual(boundingBox.maxLon);
        });
    });
});