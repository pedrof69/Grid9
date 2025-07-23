const UniformPrecisionCoordinateCompressor = require('./UniformPrecisionCoordinateCompressor');

/**
 * High-performance coordinate operations with batch processing capabilities.
 */
class CoordinateOperations {
    /**
     * Batch encodes multiple coordinate pairs for high-throughput scenarios.
     * @param {Array<{lat: number, lon: number}>} coordinates - Array of coordinate objects
     * @returns {string[]} Array of encoded Grid9 strings
     */
    static batchEncode(coordinates) {
        if (!Array.isArray(coordinates)) {
            throw new Error('Coordinates must be an array');
        }
        
        const results = new Array(coordinates.length);
        
        for (let i = 0; i < coordinates.length; i++) {
            const { lat, lon } = coordinates[i];
            if (typeof lat !== 'number' || typeof lon !== 'number') {
                throw new Error(`Invalid coordinate at index ${i}: lat and lon must be numbers`);
            }
            results[i] = UniformPrecisionCoordinateCompressor.encode(lat, lon);
        }
        
        return results;
    }

    /**
     * Batch decodes multiple encoded strings for high-throughput scenarios.
     * @param {string[]} encoded - Array of encoded Grid9 strings
     * @returns {Array<{lat: number, lon: number}>} Array of coordinate objects
     */
    static batchDecode(encoded) {
        if (!Array.isArray(encoded)) {
            throw new Error('Encoded must be an array');
        }
        
        const results = new Array(encoded.length);
        
        for (let i = 0; i < encoded.length; i++) {
            if (typeof encoded[i] !== 'string') {
                throw new Error(`Invalid encoded string at index ${i}: must be a string`);
            }
            const decoded = UniformPrecisionCoordinateCompressor.decode(encoded[i]);
            results[i] = { lat: decoded.latitude, lon: decoded.longitude };
        }
        
        return results;
    }

    /**
     * Finds all coordinates within a specified radius (in meters) of a center point.
     * Returns encoded strings of nearby coordinates.
     * @param {number} centerLat - Center latitude in degrees
     * @param {number} centerLon - Center longitude in degrees  
     * @param {number} radiusMeters - Search radius in meters
     * @param {number} [maxResults=100] - Maximum number of results to return
     * @returns {string[]} Array of encoded Grid9 strings within radius
     */
    static findNearby(centerLat, centerLon, radiusMeters, maxResults = 100) {
        if (typeof centerLat !== 'number' || typeof centerLon !== 'number') {
            throw new Error('Center coordinates must be numbers');
        }
        if (typeof radiusMeters !== 'number' || radiusMeters <= 0) {
            throw new Error('Radius must be a positive number');
        }
        if (typeof maxResults !== 'number' || maxResults <= 0) {
            throw new Error('Max results must be a positive number');
        }
        
        const results = [];
        const centerEncoded = UniformPrecisionCoordinateCompressor.encode(centerLat, centerLon);
        
        // Calculate approximate grid search bounds
        const latDelta = radiusMeters / 111320.0; // Rough conversion
        const lonDelta = radiusMeters / (111320.0 * Math.cos(centerLat * Math.PI / 180.0));
        
        const minLat = Math.max(-80, centerLat - latDelta);
        const maxLat = Math.min(80, centerLat + latDelta);
        const minLon = Math.max(-180, centerLon - lonDelta);
        const maxLon = Math.min(180, centerLon + lonDelta);
        
        // Grid search with 3-meter steps
        const latStep = 3.0 / 111320.0;
        const lonStep = 3.0 / 111320.0;
        
        for (let lat = minLat; lat <= maxLat && results.length < maxResults; lat += latStep) {
            for (let lon = minLon; lon <= maxLon && results.length < maxResults; lon += lonStep) {
                try {
                    const encoded = UniformPrecisionCoordinateCompressor.encode(lat, lon);
                    const distance = UniformPrecisionCoordinateCompressor.calculateDistance(centerEncoded, encoded);
                    
                    if (distance <= radiusMeters) {
                        results.push(encoded);
                    }
                } catch (error) {
                    // Skip invalid coordinates (out of bounds, etc.)
                    continue;
                }
            }
        }
        
        return results;
    }

    /**
     * Calculate the bounding box that contains all given coordinates
     * @param {Array<{lat: number, lon: number}>} coordinates - Array of coordinate objects
     * @returns {Object} Bounding box with minLat, maxLat, minLon, maxLon
     */
    static getBoundingBox(coordinates) {
        if (!Array.isArray(coordinates) || coordinates.length === 0) {
            throw new Error('Coordinates must be a non-empty array');
        }
        
        let minLat = Infinity;
        let maxLat = -Infinity;
        let minLon = Infinity;
        let maxLon = -Infinity;
        
        for (const coord of coordinates) {
            if (typeof coord.lat !== 'number' || typeof coord.lon !== 'number') {
                throw new Error('Each coordinate must have numeric lat and lon properties');
            }
            
            minLat = Math.min(minLat, coord.lat);
            maxLat = Math.max(maxLat, coord.lat);
            minLon = Math.min(minLon, coord.lon);
            maxLon = Math.max(maxLon, coord.lon);
        }
        
        return { minLat, maxLat, minLon, maxLon };
    }

    /**
     * Calculate the center point of a set of coordinates
     * @param {Array<{lat: number, lon: number}>} coordinates - Array of coordinate objects
     * @returns {Object} Center coordinate with lat and lon properties
     */
    static getCenterPoint(coordinates) {
        if (!Array.isArray(coordinates) || coordinates.length === 0) {
            throw new Error('Coordinates must be a non-empty array');
        }
        
        let totalLat = 0;
        let totalLon = 0;
        
        for (const coord of coordinates) {
            if (typeof coord.lat !== 'number' || typeof coord.lon !== 'number') {
                throw new Error('Each coordinate must have numeric lat and lon properties');
            }
            
            totalLat += coord.lat;
            totalLon += coord.lon;
        }
        
        return {
            lat: totalLat / coordinates.length,
            lon: totalLon / coordinates.length
        };
    }
}

module.exports = CoordinateOperations;