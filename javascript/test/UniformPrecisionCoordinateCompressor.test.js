const UniformPrecisionCoordinateCompressor = require('../src/UniformPrecisionCoordinateCompressor');

describe('UniformPrecisionCoordinateCompressor', () => {
    const PRECISION_TOLERANCE_M = 0.1; // 10cm tolerance for precision validation
    const MAX_EXPECTED_ERROR_M = 3.5; // Realistic maximum with 45-bit constraints
    const TARGET_PRECISION_M = 3.0;

    describe('encode', () => {
        test('produces nine character string', () => {
            // Test various global coordinates
            const testCoordinates = [
                { lat: 40.7128, lon: -74.0060 }, // New York
                { lat: 51.5074, lon: -0.1278 },  // London  
                { lat: 35.6762, lon: 139.6503 }, // Tokyo
                { lat: -33.8688, lon: 151.2083 }, // Sydney
                { lat: 55.7558, lon: 37.6176 },  // Moscow
                { lat: -22.9068, lon: -43.1729 }, // Rio de Janeiro
                { lat: 1.3521, lon: 103.8198 },  // Singapore
                { lat: 78.2232, lon: 15.6267 },  // Svalbard (high latitude)
                { lat: -54.8019, lon: -68.3030 }, // Ushuaia (southern)
            ];

            testCoordinates.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                
                expect(encoded).toBeDefined();
                expect(encoded).toHaveLength(9);
                
                // Verify all characters are valid base32
                for (const c of encoded) {
                    expect('0123456789ABCDEFGHJKMNPQRSTVWXYZ').toContain(c);
                }
            });
        });

        test('produces consistent results for same coordinates', () => {
            const lat = 40.7128;
            const lon = -74.0060;
            
            const encoded1 = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            const encoded2 = UniformPrecisionCoordinateCompressor.encode(lat, lon);
            
            expect(encoded1).toBe(encoded2);
        });

        test('produces different results for different coordinates', () => {
            const encoded1 = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060); // NYC
            const encoded2 = UniformPrecisionCoordinateCompressor.encode(51.5074, -0.1278);  // London
            
            expect(encoded1).not.toBe(encoded2);
        });

        test('handles boundary coordinates', () => {
            const boundaryCoords = [
                { lat: -90, lon: -180 },  // Southwest corner
                { lat: -90, lon: 180 },   // Southeast corner  
                { lat: 90, lon: -180 },   // Northwest corner
                { lat: 90, lon: 180 },    // Northeast corner
                { lat: 0, lon: 0 },       // Origin
                { lat: 0, lon: 180 },     // Antimeridian
                { lat: 0, lon: -180 },    // Antimeridian (other side)
            ];

            boundaryCoords.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                expect(encoded).toHaveLength(9);
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(true);
            });
        });

        test('throws error for invalid latitude', () => {
            expect(() => {
                UniformPrecisionCoordinateCompressor.encode(91, 0);
            }).toThrow('Latitude must be between -90 and 90');

            expect(() => {
                UniformPrecisionCoordinateCompressor.encode(-91, 0);
            }).toThrow('Latitude must be between -90 and 90');
        });

        test('throws error for invalid longitude', () => {
            expect(() => {
                UniformPrecisionCoordinateCompressor.encode(0, 181);
            }).toThrow('Longitude must be between -180 and 180');

            expect(() => {
                UniformPrecisionCoordinateCompressor.encode(0, -181);
            }).toThrow('Longitude must be between -180 and 180');
        });

        test('supports human readable format', () => {
            const encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060, true);
            
            expect(encoded).toHaveLength(11);
            expect(encoded[3]).toBe('-');
            expect(encoded[7]).toBe('-');
            expect(encoded).toMatch(/^[0-9A-Z]{3}-[0-9A-Z]{3}-[0-9A-Z]{3}$/);
        });
    });

    describe('decode', () => {
        test('decodes to approximate original coordinates', () => {
            const testCoordinates = [
                { lat: 40.7128, lon: -74.0060 }, // New York
                { lat: 51.5074, lon: -0.1278 },  // London  
                { lat: 35.6762, lon: 139.6503 }, // Tokyo
                { lat: 0, lon: 0 },               // Origin
            ];

            testCoordinates.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                const decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
                
                expect(Math.abs(decoded.latitude - lat)).toBeLessThan(0.01); // Within ~1km
                expect(Math.abs(decoded.longitude - lon)).toBeLessThan(0.01);
            });
        });

        test('handles human readable format', () => {
            const original = { lat: 40.7128, lon: -74.0060 };
            const encoded = UniformPrecisionCoordinateCompressor.encode(original.lat, original.lon, true);
            const decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
            
            expect(Math.abs(decoded.latitude - original.lat)).toBeLessThan(0.01);
            expect(Math.abs(decoded.longitude - original.lon)).toBeLessThan(0.01);
        });

        test('throws error for invalid encoded string', () => {
            expect(() => {
                UniformPrecisionCoordinateCompressor.decode('');
            }).toThrow('Encoded string cannot be null or empty');

            expect(() => {
                UniformPrecisionCoordinateCompressor.decode('TOOLONG12');
            }).toThrow(); // Could throw either length or character error

            expect(() => {
                UniformPrecisionCoordinateCompressor.decode('SHORT');
            }).toThrow('Encoded string must be 9 characters or 11-character formatted');

            expect(() => {
                UniformPrecisionCoordinateCompressor.decode('INVALID@#');
            }).toThrow('Invalid character');
            
            expect(() => {
                UniformPrecisionCoordinateCompressor.decode('INVALIDOL'); // Contains O and L which are excluded
            }).toThrow('Invalid character');
        });

        test('round trip consistency', () => {
            const testCoordinates = [
                { lat: 40.7128, lon: -74.0060 },
                { lat: 0, lon: 0 },
                { lat: -45.123, lon: 67.456 },
                { lat: 78.22, lon: -123.45 }
            ];

            testCoordinates.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                const decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
                const reencoded = UniformPrecisionCoordinateCompressor.encode(decoded.latitude, decoded.longitude);
                
                expect(reencoded).toBe(encoded);
            });
        });
    });

    describe('precision validation', () => {
        test('achieves target precision globally', () => {
            // Test precision at various latitudes
            const testLatitudes = [-80, -45, -30, 0, 30, 45, 60, 80];
            const testLongitudes = [-180, -90, 0, 90, 180];

            testLatitudes.forEach(lat => {
                testLongitudes.forEach(lon => {
                    if (lon === 180) lon = 179.9; // Avoid boundary edge case
                    
                    const precision = UniformPrecisionCoordinateCompressor.getActualPrecision(lat, lon);
                    
                    expect(precision.totalErrorM).toBeLessThanOrEqual(MAX_EXPECTED_ERROR_M);
                    expect(precision.xErrorM).toBeGreaterThan(0);
                    expect(precision.yErrorM).toBeGreaterThan(0);
                    expect(precision.totalErrorM).toBeGreaterThan(0);
                });
            });
        });

        test('precision improves toward poles for longitude', () => {
            const longitude = 0;
            
            const equatorPrecision = UniformPrecisionCoordinateCompressor.getActualPrecision(0, longitude);
            const polarPrecision = UniformPrecisionCoordinateCompressor.getActualPrecision(80, longitude);
            
            expect(polarPrecision.xErrorM).toBeLessThan(equatorPrecision.xErrorM);
        });
    });

    describe('distance calculation', () => {
        test('calculates correct distance between known coordinates', () => {
            const nycEncoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
            const londonEncoded = UniformPrecisionCoordinateCompressor.encode(51.5074, -0.1278);
            
            const distance = UniformPrecisionCoordinateCompressor.calculateDistance(nycEncoded, londonEncoded);
            
            // Expected distance is approximately 5570 km
            expect(distance).toBeGreaterThan(5500000); // 5500 km
            expect(distance).toBeLessThan(5600000);    // 5600 km
        });

        test('distance from point to itself is zero', () => {
            const encoded = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
            const distance = UniformPrecisionCoordinateCompressor.calculateDistance(encoded, encoded);
            
            expect(distance).toBe(0);
        });
    });

    describe('validation', () => {
        test('validates correct encodings', () => {
            const validEncodings = [
                'Q7KH2BBYF',
                'S50MBZX2Y',
                'PAYMZ39T7',
                '000000000',
                'ZZZZZZZZZ'
            ];

            validEncodings.forEach(encoded => {
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(true);
            });
        });

        test('validates human readable format', () => {
            const validFormatted = [
                'Q7K-H2B-BYF',
                'S50-MBZ-X2Y',
                '000-000-000'
            ];

            validFormatted.forEach(encoded => {
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(true);
            });
        });

        test('rejects invalid encodings', () => {
            const invalidEncodings = [
                '',
                'SHORT',
                'TOOLONG123',
                'INVALID@#',
                'ABC-DEF',      // Wrong format
                'ABC-DEF-GHI-J', // Too long with dashes
                null,
                undefined
            ];

            invalidEncodings.forEach(encoded => {
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(encoded)).toBe(false);
            });
        });
    });

    describe('formatting', () => {
        test('formats for human readability', () => {
            const code = 'Q7KH2BBYF';
            const formatted = UniformPrecisionCoordinateCompressor.formatForHumans(code);
            
            expect(formatted).toBe('Q7K-H2B-BYF');
            expect(formatted).toHaveLength(11);
        });

        test('removes formatting', () => {
            const formatted = 'Q7K-H2B-BYF';
            const unformatted = UniformPrecisionCoordinateCompressor.removeFormatting(formatted);
            
            expect(unformatted).toBe('Q7KH2BBYF');
            expect(unformatted).toHaveLength(9);
        });

        test('detects human readable format', () => {
            expect(UniformPrecisionCoordinateCompressor.isFormattedForHumans('Q7K-H2B-BYF')).toBe(true);
            expect(UniformPrecisionCoordinateCompressor.isFormattedForHumans('Q7KH2BBYF')).toBe(false);
            expect(UniformPrecisionCoordinateCompressor.isFormattedForHumans('ABC')).toBe(false);
        });

        test('throws error for invalid format operations', () => {
            expect(() => {
                UniformPrecisionCoordinateCompressor.formatForHumans('SHORT');
            }).toThrow('Input must be exactly 9 characters');

            expect(() => {
                UniformPrecisionCoordinateCompressor.removeFormatting('INVALID');
            }).toThrow('Input must be in XXX-XXX-XXX format');
        });
    });

    describe('neighbors', () => {
        test('generates neighboring coordinates', () => {
            const center = UniformPrecisionCoordinateCompressor.encode(40.7128, -74.0060);
            const neighbors = UniformPrecisionCoordinateCompressor.getNeighbors(center);
            
            expect(neighbors).toBeInstanceOf(Array);
            expect(neighbors.length).toBeGreaterThan(0);
            expect(neighbors.length).toBeLessThanOrEqual(8);
            
            // All neighbors should be valid encodings
            neighbors.forEach(neighbor => {
                expect(UniformPrecisionCoordinateCompressor.isValidEncoding(neighbor)).toBe(true);
                expect(neighbor).not.toBe(center); // Should not include center
            });
        });

        test('neighbors are nearby', () => {
            const center = UniformPrecisionCoordinateCompressor.encode(0, 0); // Equator
            const neighbors = UniformPrecisionCoordinateCompressor.getNeighbors(center);
            
            neighbors.forEach(neighbor => {
                const distance = UniformPrecisionCoordinateCompressor.calculateDistance(center, neighbor);
                expect(distance).toBeLessThan(10000); // Should be within 10km
                expect(distance).toBeGreaterThan(0);   // Should not be zero
            });
        });
    });

    describe('edge cases', () => {
        test('handles coordinates near poles', () => {
            const polarCoords = [
                { lat: 89.999, lon: 0 },
                { lat: -89.999, lon: 0 },
                { lat: 89, lon: 179 },
                { lat: -89, lon: -179 }
            ];

            polarCoords.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                expect(encoded).toHaveLength(9);
                
                const decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
                expect(Math.abs(decoded.latitude - lat)).toBeLessThan(1.0);
                expect(Math.abs(decoded.longitude - lon)).toBeLessThan(1.0);
            });
        });

        test('handles coordinates near antimeridian', () => {
            const antimeridianCoords = [
                { lat: 0, lon: 179.999 },
                { lat: 0, lon: -179.999 },
                { lat: 45, lon: 180 },
                { lat: -45, lon: -180 }
            ];

            antimeridianCoords.forEach(({ lat, lon }) => {
                const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                expect(encoded).toHaveLength(9);
                
                const decoded = UniformPrecisionCoordinateCompressor.decode(encoded);
                expect(Math.abs(decoded.latitude - lat)).toBeLessThan(0.1);
            });
        });

        test('handles very small coordinate differences', () => {
            const base = { lat: 40.7128, lon: -74.0060 };
            const nearby = { lat: 40.7128001, lon: -74.0060001 }; // ~10cm difference
            
            const encoded1 = UniformPrecisionCoordinateCompressor.encode(base.lat, base.lon);
            const encoded2 = UniformPrecisionCoordinateCompressor.encode(nearby.lat, nearby.lon);
            
            // May or may not be the same encoding depending on grid alignment
            expect(encoded1).toHaveLength(9);
            expect(encoded2).toHaveLength(9);
        });
    });

    describe('performance characteristics', () => {
        test('encodes many coordinates efficiently', () => {
            const startTime = Date.now();
            const iterations = 1000;
            
            for (let i = 0; i < iterations; i++) {
                const lat = (Math.random() - 0.5) * 160; // -80 to 80
                const lon = (Math.random() - 0.5) * 360; // -180 to 180
                UniformPrecisionCoordinateCompressor.encode(lat, lon);
            }
            
            const endTime = Date.now();
            const duration = endTime - startTime;
            
            // Should complete 1000 operations in reasonable time (< 1 second)
            expect(duration).toBeLessThan(1000);
        });

        test('decodes many coordinates efficiently', () => {
            // Pre-generate test data
            const encodedCoords = [];
            for (let i = 0; i < 1000; i++) {
                const lat = (Math.random() - 0.5) * 160;
                const lon = (Math.random() - 0.5) * 360;
                encodedCoords.push(UniformPrecisionCoordinateCompressor.encode(lat, lon));
            }
            
            const startTime = Date.now();
            
            encodedCoords.forEach(encoded => {
                UniformPrecisionCoordinateCompressor.decode(encoded);
            });
            
            const endTime = Date.now();
            const duration = endTime - startTime;
            
            // Should complete 1000 operations in reasonable time
            expect(duration).toBeLessThan(1000);
        });
    });
});