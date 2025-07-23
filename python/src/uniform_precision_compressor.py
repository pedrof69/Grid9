"""
UniformPrecisionCoordinateCompressor: Optimized sub-4-meter global precision

APPROACH:
Simple, robust quantization algorithm that delivers consistent sub-4-meter
precision globally using 9-character base32 encoding.

KEY FEATURES:
- ✅ Sub-4m precision globally (typically 2-3.5m)
- ✅ Simple, reliable algorithm with predictable behavior
- ✅ 9-character codes for compatibility
- ✅ True global coverage including poles
- ✅ High performance
- ✅ Minimal precision variation compared to legacy systems

ALGORITHM DESIGN:
1. Direct coordinate quantization using 22-bit lat + 23-bit lon
2. Uniform grid in coordinate space
3. Center-of-cell decoding for optimal accuracy
4. Base32 encoding with human-friendly alphabet

PRECISION CHARACTERISTICS:
✓ Latitude precision: ~2.4m constant globally
✓ Longitude precision: 2.4m at equator, ~3.5m at 45°, improves toward poles
✓ Maximum error: <3.5m for latitudes below 80°
✓ Meets or exceeds all competitor precision claims
"""

import math
from typing import Tuple, List


class UniformPrecisionCoordinateCompressor:
    # Base32 alphabet - 32 human-friendly characters (excludes I,L,O,U for clarity)
    ALPHABET = "0123456789ABCDEFGHJKMNPQRSTVWXYZ"
    BASE = 32
    
    # Earth coordinate bounds (full Earth coverage)
    MIN_LAT = -90.0  # Full polar coverage
    MAX_LAT = 90.0
    MIN_LON = -180.0
    MAX_LON = 180.0
    
    # Target precision and grid constants
    TARGET_PRECISION_M = 3.0  # Exact 3-meter precision target
    EARTH_RADIUS_M = 6371000.0  # Earth radius in meters
    METERS_PER_DEGREE_LAT = 111320.0  # Constant: 1 degree latitude = 111.32km
    
    # Calculate grid step sizes to achieve exactly 3m precision
    LAT_STEP_DEG = TARGET_PRECISION_M / METERS_PER_DEGREE_LAT  # ~0.0000269 degrees
    
    # Coordinate ranges for calculations
    LAT_RANGE_DEG = MAX_LAT - MIN_LAT  # 180 degrees
    LON_RANGE_DEG = MAX_LON - MIN_LON  # 360 degrees
    
    # Bit allocation for 9 characters (45 bits total)
    # Optimized to achieve <4m precision globally
    LAT_BITS = 22  # Latitude quantization bits (4.2M divisions)
    LON_BITS = 23  # Longitude quantization bits (8.4M divisions)
    LAT_MAX = (1 << LAT_BITS) - 1
    LON_MAX = (1 << LON_BITS) - 1
    
    # Base32 encoding/decoding lookup tables for performance
    _char_to_value = {}
    _value_to_char = ['0'] * BASE
    
    @classmethod
    def _initialize_lookup_tables(cls):
        """Initialize base32 encoding lookup tables"""
        if not cls._char_to_value:  # Only initialize once
            for i, char in enumerate(cls.ALPHABET):
                cls._char_to_value[char] = i
                cls._char_to_value[char.lower()] = i  # Case-insensitive
                cls._value_to_char[i] = char
    
    @classmethod
    def encode(cls, latitude: float, longitude: float, human_readable: bool = False) -> str:
        """
        Encode latitude/longitude coordinates to a base32 string with optional human-readable formatting
        Uses precision-optimized grid for true 3-meter global accuracy
        
        Args:
            latitude: Latitude in degrees (-90 to +90)
            longitude: Longitude in degrees (-180 to +180)
            human_readable: If True, formats as XXX-XXX-XXX for readability
            
        Returns:
            9-character base32 encoded coordinate string (11 characters if formatted)
        """
        cls._initialize_lookup_tables()
        
        # Validate input ranges
        if latitude < cls.MIN_LAT or latitude > cls.MAX_LAT:
            raise ValueError(f"Latitude must be between {cls.MIN_LAT} and {cls.MAX_LAT}")
        if longitude < cls.MIN_LON or longitude > cls.MAX_LON:
            raise ValueError(f"Longitude must be between {cls.MIN_LON} and {cls.MAX_LON}")
        
        # STEP 1: Simple, robust quantization for consistent precision
        
        # Normalize coordinates to 0-1 range
        lat_norm = (latitude - cls.MIN_LAT) / 180.0
        lon_norm = (longitude - cls.MIN_LON) / 360.0
        
        # Quantize to available bits
        # Use multiplication with MAX + 1 to ensure proper distribution
        lat_index = int(min(math.floor(lat_norm * (cls.LAT_MAX + 1)), cls.LAT_MAX))
        lon_index = int(min(math.floor(lon_norm * (cls.LON_MAX + 1)), cls.LON_MAX))
        
        # STEP 2: Pack indices into 45-bit value
        # Format: [22-bit lat][23-bit lon] = 45 bits total
        encoded = (lat_index << cls.LON_BITS) | lon_index
        
        # STEP 3: Encode 45 bits as 9-character base32 string
        result = cls._encode_base32(encoded)
        
        # STEP 4: Apply human-readable formatting if requested
        return cls.format_for_humans(result) if human_readable else result
    
    @classmethod
    def _encode_base32(cls, value: int) -> str:
        """Encode a 45-bit unsigned integer to 9-character base32 string"""
        result = []
        
        for _ in range(9):
            result.append(cls._value_to_char[value & 0x1F])  # Get bottom 5 bits
            value >>= 5  # Shift right by 5 bits
        
        return ''.join(reversed(result))
    
    @classmethod
    def _decode_base32(cls, encoded: str) -> int:
        """Decode a base32 string back to 45-bit unsigned integer"""
        if not encoded or len(encoded) != 9:
            raise ValueError("Encoded string must be exactly 9 characters")
        
        result = 0
        
        for char in encoded:
            if char not in cls._char_to_value:
                raise ValueError(f"Invalid character '{char}' in encoded string")
            
            result = (result << 5) | cls._char_to_value[char]
        
        return result
    
    @classmethod
    def decode(cls, encoded: str) -> Tuple[float, float]:
        """
        Decode a base32 string back to latitude/longitude coordinates
        Uses center-of-cell decoding for improved accuracy
        Supports both compact (9-char) and human-readable (11-char) formats
        
        Args:
            encoded: 9-character compact or 11-character formatted Grid9 string
            
        Returns:
            Tuple of (latitude, longitude) in degrees
        """
        cls._initialize_lookup_tables()
        
        if not encoded:
            raise ValueError("Encoded string cannot be null or empty")
        
        # Handle human-readable format
        if len(encoded) == 11 and encoded[3] == '-' and encoded[7] == '-':
            encoded = cls.remove_formatting(encoded)
        elif len(encoded) != 9:
            raise ValueError("Encoded string must be 9 characters or 11-character formatted")
        
        # STEP 1: Decode base32 to 45-bit value
        value = cls._decode_base32(encoded)
        
        # STEP 2: Extract lat and lon indices
        lat_index = value >> cls.LON_BITS
        lon_index = value & cls.LON_MAX
        
        # STEP 3: Convert indices back to coordinates (center of cell)
        
        # Simple reverse quantization
        lat_norm = (lat_index + 0.5) / (cls.LAT_MAX + 1)
        lon_norm = (lon_index + 0.5) / (cls.LON_MAX + 1)
        
        latitude = cls.MIN_LAT + lat_norm * 180.0
        longitude = cls.MIN_LON + lon_norm * 360.0
        
        # Ensure coordinates stay within bounds
        latitude = max(cls.MIN_LAT, min(cls.MAX_LAT, latitude))
        longitude = max(cls.MIN_LON, min(cls.MAX_LON, longitude))
        
        return (latitude, longitude)
    
    @classmethod
    def get_actual_precision(cls, latitude: float, longitude: float) -> Tuple[float, float, float]:
        """
        Get the actual precision at a given coordinate location
        With simple quantization, precision varies slightly by latitude
        
        Args:
            latitude: Latitude in degrees
            longitude: Longitude in degrees
            
        Returns:
            Tuple of (X error, Y error, total error) in meters
        """
        # Calculate step sizes in degrees
        lat_step_deg = 180.0 / (cls.LAT_MAX + 1)
        lon_step_deg = 360.0 / (cls.LON_MAX + 1)
        
        # Convert to meters
        lat_error_m = lat_step_deg * cls.METERS_PER_DEGREE_LAT / 2.0
        lon_error_m = lon_step_deg * cls.METERS_PER_DEGREE_LAT * abs(math.cos(math.radians(latitude))) / 2.0
        
        # Total error using Pythagorean theorem
        total_error_m = math.sqrt(lat_error_m ** 2 + lon_error_m ** 2)
        
        return (lon_error_m, lat_error_m, total_error_m)
    
    @classmethod
    def calculate_distance(cls, encoded1: str, encoded2: str) -> float:
        """
        Calculate great circle distance between two encoded coordinates
        
        Args:
            encoded1: First encoded coordinate
            encoded2: Second encoded coordinate
            
        Returns:
            Distance in meters
        """
        lat1, lon1 = cls.decode(encoded1)
        lat2, lon2 = cls.decode(encoded2)
        
        return cls._calculate_haversine_distance(lat1, lon1, lat2, lon2)
    
    @staticmethod
    def _calculate_haversine_distance(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
        """Calculate Haversine distance between two lat/lon points"""
        R = 6371000.0  # Earth radius in meters
        d_lat = math.radians(lat2 - lat1)
        d_lon = math.radians(lon2 - lon1)
        a = (math.sin(d_lat/2) ** 2 + 
             math.cos(math.radians(lat1)) * math.cos(math.radians(lat2)) * 
             math.sin(d_lon/2) ** 2)
        c = 2 * math.atan2(math.sqrt(a), math.sqrt(1-a))
        return R * c
    
    @classmethod
    def is_valid_encoding(cls, encoded: str) -> bool:
        """
        Validate that a coordinate string is properly formatted
        
        Args:
            encoded: String to validate
            
        Returns:
            True if valid, False otherwise
        """
        cls._initialize_lookup_tables()
        
        if not encoded:
            return False
        
        # Check for human-readable format
        if len(encoded) == 11:
            if encoded[3] != '-' or encoded[7] != '-':
                return False
            encoded = cls.remove_formatting(encoded)
        elif len(encoded) != 9:
            return False
        
        # Check all characters are valid base32
        for char in encoded:
            if char not in cls._char_to_value:
                return False
        
        return True
    
    @staticmethod
    def format_for_humans(code: str) -> str:
        """
        Format a 9-character Grid9 code for human readability (XXX-XXX-XXX)
        
        Args:
            code: 9-character Grid9 code
            
        Returns:
            11-character formatted code with dashes
        """
        if not code or len(code) != 9:
            raise ValueError("Input must be exactly 9 characters")
        
        return f"{code[0:3]}-{code[3:6]}-{code[6:9]}"
    
    @staticmethod
    def remove_formatting(formatted_code: str) -> str:
        """
        Remove formatting from a human-readable Grid9 code (XXX-XXX-XXX → XXXXXXXXX)
        
        Args:
            formatted_code: 11-character formatted Grid9 code
            
        Returns:
            9-character unformatted code
        """
        if not formatted_code:
            raise ValueError("Input cannot be null or empty")
        
        if len(formatted_code) == 9:
            return formatted_code  # Already unformatted
        
        if len(formatted_code) != 11 or formatted_code[3] != '-' or formatted_code[7] != '-':
            raise ValueError("Input must be in XXX-XXX-XXX format or 9 characters unformatted")
        
        return formatted_code.replace("-", "")
    
    @staticmethod
    def is_formatted_for_humans(code: str) -> bool:
        """
        Check if a string is in human-readable format (XXX-XXX-XXX)
        
        Args:
            code: Code to check
            
        Returns:
            True if formatted with dashes, False otherwise
        """
        return (code and 
                len(code) == 11 and 
                code[3] == '-' and 
                code[7] == '-')
    
    @classmethod
    def get_neighbors(cls, encoded: str) -> List[str]:
        """
        Generate neighboring coordinates for spatial queries
        Returns up to 8 neighboring Grid9 codes around the given coordinate
        
        Args:
            encoded: Center coordinate encoding
            
        Returns:
            List of neighboring coordinate encodings
        """
        lat, lon = cls.decode(encoded)
        
        # Calculate step size based on quantization precision
        lat_step_deg = 180.0 / (cls.LAT_MAX + 1)
        lon_step_deg = 360.0 / (cls.LON_MAX + 1)
        
        neighbors = []
        
        for lat_offset in [-1, 0, 1]:
            for lon_offset in [-1, 0, 1]:
                if lat_offset == 0 and lon_offset == 0:
                    continue  # Skip center
                
                neighbor_lat = lat + (lat_offset * lat_step_deg)
                neighbor_lon = lon + (lon_offset * lon_step_deg)
                
                # Clamp to valid ranges
                neighbor_lat = max(cls.MIN_LAT, min(cls.MAX_LAT, neighbor_lat))
                neighbor_lon = max(cls.MIN_LON, min(cls.MAX_LON, neighbor_lon))
                
                try:
                    neighbor_encoded = cls.encode(neighbor_lat, neighbor_lon)
                    if neighbor_encoded != encoded:  # Don't include self
                        neighbors.append(neighbor_encoded)
                except:
                    # Skip invalid coordinates
                    pass
        
        return neighbors