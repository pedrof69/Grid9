/**
 * Grid9 TypeScript Definitions
 * 
 * Type definitions for the Grid9 coordinate compression library.
 */

export interface Coordinate {
    latitude: number;
    longitude: number;
}

export interface CoordinateInput {
    lat: number;
    lon: number;
}

export interface PrecisionInfo {
    xErrorM: number;
    yErrorM: number;
    totalErrorM: number;
}

export interface BoundingBox {
    minLat: number;
    maxLat: number;
    minLon: number;
    maxLon: number;
}

export interface CenterPoint {
    lat: number;
    lon: number;
}

/**
 * Main Grid9 coordinate compression class
 */
export class UniformPrecisionCoordinateCompressor {
    /**
     * Encode coordinates to Grid9 string
     */
    static encode(latitude: number, longitude: number, humanReadable?: boolean): string;
    
    /**
     * Decode Grid9 string to coordinates
     */
    static decode(encoded: string): Coordinate;
    
    /**
     * Get precision information for coordinates
     */
    static getActualPrecision(latitude: number, longitude: number): PrecisionInfo;
    
    /**
     * Calculate distance between two Grid9 codes
     */
    static calculateDistance(encoded1: string, encoded2: string): number;
    
    /**
     * Validate Grid9 code format
     */
    static isValidEncoding(encoded: string): boolean;
    
    /**
     * Format code for human readability
     */
    static formatForHumans(code: string): string;
    
    /**
     * Remove formatting from code
     */
    static removeFormatting(formattedCode: string): string;
    
    /**
     * Check if code is formatted
     */
    static isFormattedForHumans(code: string): boolean;
    
    /**
     * Get neighboring Grid9 codes
     */
    static getNeighbors(encoded: string): string[];
}

/**
 * Helper class for batch and spatial operations
 */
export class CoordinateOperations {
    /**
     * Batch encode coordinates
     */
    static batchEncode(coordinates: CoordinateInput[]): string[];
    
    /**
     * Batch decode Grid9 codes
     */
    static batchDecode(encoded: string[]): CoordinateInput[];
    
    /**
     * Find coordinates within radius
     */
    static findNearby(centerLat: number, centerLon: number, radiusMeters: number, maxResults?: number): string[];
    
    /**
     * Calculate bounding box of coordinates
     */
    static getBoundingBox(coordinates: CoordinateInput[]): BoundingBox;
    
    /**
     * Calculate center point of coordinates
     */
    static getCenterPoint(coordinates: CoordinateInput[]): CenterPoint;
}

// Convenience function exports
export function encode(latitude: number, longitude: number, humanReadable?: boolean): string;
export function decode(encoded: string): Coordinate;
export function isValid(encoded: string): boolean;
export function distance(encoded1: string, encoded2: string): number;
export function precision(latitude: number, longitude: number): PrecisionInfo;
export function neighbors(encoded: string): string[];
export function format(code: string): string;
export function unformat(formattedCode: string): string;

// Batch operation exports
export function batchEncode(coordinates: CoordinateInput[]): string[];
export function batchDecode(encoded: string[]): CoordinateInput[];
export function findNearby(centerLat: number, centerLon: number, radiusMeters: number, maxResults?: number): string[];
export function getBoundingBox(coordinates: CoordinateInput[]): BoundingBox;
export function getCenterPoint(coordinates: CoordinateInput[]): CenterPoint;

// Export classes
export { UniformPrecisionCoordinateCompressor, CoordinateOperations };