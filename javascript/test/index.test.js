const Grid9 = require('../src/index');

describe('Grid9 Module Exports', () => {
    test('exports main classes', () => {
        expect(Grid9.UniformPrecisionCoordinateCompressor).toBeDefined();
        expect(Grid9.CoordinateOperations).toBeDefined();
    });

    test('exports convenience functions', () => {
        expect(typeof Grid9.encode).toBe('function');
        expect(typeof Grid9.decode).toBe('function');
        expect(typeof Grid9.isValid).toBe('function');
        expect(typeof Grid9.distance).toBe('function');
        expect(typeof Grid9.precision).toBe('function');
        expect(typeof Grid9.neighbors).toBe('function');
        expect(typeof Grid9.format).toBe('function');
        expect(typeof Grid9.unformat).toBe('function');
    });

    test('exports batch operations', () => {
        expect(typeof Grid9.batchEncode).toBe('function');
        expect(typeof Grid9.batchDecode).toBe('function');
        expect(typeof Grid9.findNearby).toBe('function');
        expect(typeof Grid9.getBoundingBox).toBe('function');
        expect(typeof Grid9.getCenterPoint).toBe('function');
    });

    describe('convenience function integration', () => {
        test('encode and decode work correctly', () => {
            const lat = 40.7128;
            const lon = -74.0060;
            
            const encoded = Grid9.encode(lat, lon);
            const decoded = Grid9.decode(encoded);
            
            expect(encoded).toHaveLength(9);
            expect(Math.abs(decoded.latitude - lat)).toBeLessThan(0.01);
            expect(Math.abs(decoded.longitude - lon)).toBeLessThan(0.01);
        });

        test('validation works correctly', () => {
            const encoded = Grid9.encode(40.7128, -74.0060);
            expect(Grid9.isValid(encoded)).toBe(true);
            expect(Grid9.isValid('INVALID@#')).toBe(false);
        });

        test('distance calculation works correctly', () => {
            const nyc = Grid9.encode(40.7128, -74.0060);
            const london = Grid9.encode(51.5074, -0.1278);
            
            const distance = Grid9.distance(nyc, london);
            expect(distance).toBeGreaterThan(5000000); // > 5000km
            expect(distance).toBeLessThan(6000000);    // < 6000km
        });

        test('precision calculation works correctly', () => {
            const precision = Grid9.precision(40.7128, -74.0060);
            
            expect(precision.xErrorM).toBeGreaterThan(0);
            expect(precision.yErrorM).toBeGreaterThan(0);
            expect(precision.totalErrorM).toBeGreaterThan(0);
            expect(precision.totalErrorM).toBeLessThan(5); // Should be under 5m
        });

        test('neighbors function works correctly', () => {
            const center = Grid9.encode(40.7128, -74.0060);
            const neighbors = Grid9.neighbors(center);
            
            expect(Array.isArray(neighbors)).toBe(true);
            expect(neighbors.length).toBeGreaterThan(0);
            expect(neighbors.every(n => Grid9.isValid(n))).toBe(true);
        });

        test('formatting functions work correctly', () => {
            const code = 'Q7KH2BBYF';
            const formatted = Grid9.format(code);
            const unformatted = Grid9.unformat(formatted);
            
            expect(formatted).toBe('Q7K-H2B-BYF');
            expect(unformatted).toBe(code);
        });

        test('batch operations work correctly', () => {
            const coordinates = [
                { lat: 40.7128, lon: -74.0060 },
                { lat: 51.5074, lon: -0.1278 }
            ];
            
            const encoded = Grid9.batchEncode(coordinates);
            const decoded = Grid9.batchDecode(encoded);
            
            expect(encoded).toHaveLength(2);
            expect(decoded).toHaveLength(2);
            expect(encoded.every(e => Grid9.isValid(e))).toBe(true);
        });

        test('spatial operations work correctly', () => {
            const coordinates = [
                { lat: 40.7, lon: -74.0 },
                { lat: 40.8, lon: -74.1 }
            ];
            
            const bbox = Grid9.getBoundingBox(coordinates);
            const center = Grid9.getCenterPoint(coordinates);
            const nearby = Grid9.findNearby(40.75, -74.05, 10000, 5);
            
            expect(bbox).toHaveProperty('minLat');
            expect(bbox).toHaveProperty('maxLat');
            expect(bbox).toHaveProperty('minLon');
            expect(bbox).toHaveProperty('maxLon');
            
            expect(center).toHaveProperty('lat');
            expect(center).toHaveProperty('lon');
            
            expect(Array.isArray(nearby)).toBe(true);
        });
    });

    describe('error handling consistency', () => {
        test('convenience functions throw same errors as main classes', () => {
            expect(() => Grid9.encode(91, 0)).toThrow();
            expect(() => Grid9.decode('INVALID')).toThrow();
            expect(() => Grid9.format('SHORT')).toThrow();
            expect(() => Grid9.unformat('INVALID')).toThrow();
        });
    });
});