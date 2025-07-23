"""
Grid9 - Precision Coordinate Compression
A coordinate compression system featuring 9-character Grid9 codes with uniform 3-meter precision globally
"""

from .uniform_precision_compressor import UniformPrecisionCoordinateCompressor
from .meter_based_compressor import MeterBasedCoordinateCompressor
from .coordinate_operations import CoordinateOperations

__version__ = "1.0.0"
__all__ = [
    "UniformPrecisionCoordinateCompressor",
    "MeterBasedCoordinateCompressor", 
    "CoordinateOperations"
]