/**
 * Grid9 - Uniform Global Precision Coordinate Compression
 * 
 * A JavaScript library for encoding and decoding coordinates using the Grid9 system,
 * which provides uniform 3-meter precision globally with 9-character codes.
 * 
 * @author Grid9 Team
 * @version 1.0.0
 * @license MIT
 */

const UniformPrecisionCoordinateCompressor = require('./UniformPrecisionCoordinateCompressor');
const CoordinateOperations = require('./CoordinateOperations');

// Export main classes
module.exports = {
    UniformPrecisionCoordinateCompressor,
    CoordinateOperations,
    
    // Convenience aliases for easier usage
    encode: UniformPrecisionCoordinateCompressor.encode.bind(UniformPrecisionCoordinateCompressor),
    decode: UniformPrecisionCoordinateCompressor.decode.bind(UniformPrecisionCoordinateCompressor),
    isValid: UniformPrecisionCoordinateCompressor.isValidEncoding.bind(UniformPrecisionCoordinateCompressor),
    distance: UniformPrecisionCoordinateCompressor.calculateDistance.bind(UniformPrecisionCoordinateCompressor),
    precision: UniformPrecisionCoordinateCompressor.getActualPrecision.bind(UniformPrecisionCoordinateCompressor),
    neighbors: UniformPrecisionCoordinateCompressor.getNeighbors.bind(UniformPrecisionCoordinateCompressor),
    format: UniformPrecisionCoordinateCompressor.formatForHumans.bind(UniformPrecisionCoordinateCompressor),
    unformat: UniformPrecisionCoordinateCompressor.removeFormatting.bind(UniformPrecisionCoordinateCompressor),
    
    // Batch operations
    batchEncode: CoordinateOperations.batchEncode.bind(CoordinateOperations),
    batchDecode: CoordinateOperations.batchDecode.bind(CoordinateOperations),
    findNearby: CoordinateOperations.findNearby.bind(CoordinateOperations),
    getBoundingBox: CoordinateOperations.getBoundingBox.bind(CoordinateOperations),
    getCenterPoint: CoordinateOperations.getCenterPoint.bind(CoordinateOperations)
};