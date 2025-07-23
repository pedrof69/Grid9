//! Uniform precision coordinate compression implementation

use crate::{Grid9Error, Result};

const BASE32_ALPHABET: &[u8] = b"0123456789ABCDEFGHJKMNPQRSTVWXYZ";
const LAT_BITS: u32 = 22;
const LON_BITS: u32 = 23;
const LAT_MAX: u64 = (1u64 << LAT_BITS) - 1;
const LON_MAX: u64 = (1u64 << LON_BITS) - 1;
const EARTH_RADIUS_M: f64 = 6_371_000.0;

/// Precision information for a coordinate
#[derive(Debug, Clone, PartialEq)]
#[cfg_attr(feature = "serde", derive(serde::Serialize, serde::Deserialize))]
pub struct PrecisionInfo {
    /// Latitude error in meters
    pub lat_error_m: f64,
    /// Longitude error in meters  
    pub lon_error_m: f64,
    /// Total error in meters (diagonal of error rectangle)
    pub total_error_m: f64,
}

/// Encodes latitude and longitude coordinates to a Grid9 string.
///
/// # Arguments
/// * `latitude` - Latitude in degrees (-90 to 90)
/// * `longitude` - Longitude in degrees (-180 to 180)
/// * `human_readable` - If true, returns XXX-XXX-XXX format with dashes
///
/// # Returns
/// A 9-character Grid9 code (or 11 characters with dashes if human_readable is true)
///
/// # Example
/// ```rust
/// use grid9::encode;
/// 
/// let code = encode(40.7128, -74.0060, false)?; // "Q7KH2BBYF"
/// let readable = encode(40.7128, -74.0060, true)?; // "Q7K-H2B-BYF"
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn encode(latitude: f64, longitude: f64, human_readable: bool) -> Result<String> {
    validate_coordinates(latitude, longitude)?;
    
    // Normalize coordinates to [0, 1] range
    let norm_lat = (latitude + 90.0) / 180.0;
    let norm_lon = (longitude + 180.0) / 360.0;
    
    // Quantize to bit precision
    let lat_bits = ((norm_lat * LAT_MAX as f64) as u64).min(LAT_MAX);
    let lon_bits = ((norm_lon * LON_MAX as f64) as u64).min(LON_MAX);
    
    // Pack into 45-bit value
    let packed = (lat_bits << LON_BITS) | lon_bits;
    
    // Convert to base32 (9 characters for 45 bits)
    let mut result = String::with_capacity(if human_readable { 11 } else { 9 });
    let mut temp = packed;
    
    for _ in 0..9 {
        let index = (temp & 0x1F) as usize;
        result.insert(0, BASE32_ALPHABET[index] as char);
        temp >>= 5;
    }
    
    if human_readable {
        Ok(format_for_humans(&result))
    } else {
        Ok(result)
    }
}

/// Decodes a Grid9 string to latitude and longitude coordinates.
///
/// # Arguments
/// * `encoded` - A 9-character Grid9 code (dashes are automatically removed)
///
/// # Returns
/// A tuple of (latitude, longitude) in degrees
///
/// # Example
/// ```rust
/// use grid9::decode;
/// 
/// let (lat, lon) = decode("Q7KH2BBYF")?; // (40.712779, -74.005988)
/// let (lat2, lon2) = decode("Q7K-H2B-BYF")?; // Same result
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn decode(encoded: &str) -> Result<(f64, f64)> {
    let clean_encoded = remove_formatting(encoded);
    validate_encoded_string(&clean_encoded)?;
    
    // Convert from base32 to 45-bit value
    let mut packed = 0u64;
    for ch in clean_encoded.chars() {
        packed <<= 5;
        let pos = BASE32_ALPHABET.iter().position(|&b| b as char == ch)
            .ok_or(Grid9Error::InvalidCharacter(ch))?;
        packed |= pos as u64;
    }
    
    // Extract latitude and longitude bits
    let lon_bits = packed & LON_MAX;
    let lat_bits = (packed >> LON_BITS) & LAT_MAX;
    
    // Denormalize coordinates
    let norm_lat = lat_bits as f64 / LAT_MAX as f64;
    let norm_lon = lon_bits as f64 / LON_MAX as f64;
    
    let latitude = norm_lat * 180.0 - 90.0;
    let longitude = norm_lon * 360.0 - 180.0;
    
    Ok((latitude, longitude))
}

/// Calculates the distance between two Grid9 codes in meters.
///
/// # Arguments
/// * `encoded1` - First Grid9 code
/// * `encoded2` - Second Grid9 code
///
/// # Returns
/// Distance in meters using the Haversine formula
///
/// # Example
/// ```rust
/// use grid9::{encode, calculate_distance};
/// 
/// let nyc = encode(40.7128, -74.0060, false)?;
/// let london = encode(51.5074, -0.1278, false)?;
/// let distance = calculate_distance(&nyc, &london)?; // ~5,570,224 meters
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn calculate_distance(encoded1: &str, encoded2: &str) -> Result<f64> {
    let (lat1, lon1) = decode(encoded1)?;
    let (lat2, lon2) = decode(encoded2)?;
    
    Ok(haversine_distance(lat1, lon1, lat2, lon2))
}

/// Validates if a string is a valid Grid9 encoding.
///
/// # Arguments
/// * `encoded` - String to validate
///
/// # Returns
/// True if the string is a valid Grid9 code (with or without dashes)
///
/// # Example
/// ```rust
/// use grid9::is_valid_encoding;
/// 
/// assert!(is_valid_encoding("Q7KH2BBYF"));     // Valid
/// assert!(is_valid_encoding("Q7K-H2B-BYF"));   // Valid with dashes
/// assert!(!is_valid_encoding("INVALID"));      // Invalid
/// ```
pub fn is_valid_encoding(encoded: &str) -> bool {
    let clean_encoded = remove_formatting(encoded);
    validate_encoded_string(&clean_encoded).is_ok()
}

/// Formats a compact Grid9 code with dashes for human readability.
///
/// # Arguments
/// * `encoded` - A 9-character Grid9 code
///
/// # Returns
/// Formatted string in XXX-XXX-XXX format
///
/// # Example
/// ```rust
/// use grid9::format_for_humans;
/// 
/// let formatted = format_for_humans("Q7KH2BBYF"); // "Q7K-H2B-BYF"
/// ```
pub fn format_for_humans(encoded: &str) -> String {
    if encoded.len() != 9 {
        return encoded.to_string();
    }
    
    format!("{}-{}-{}", &encoded[0..3], &encoded[3..6], &encoded[6..9])
}

/// Removes formatting dashes from a Grid9 code.
///
/// # Arguments
/// * `formatted` - Grid9 code with or without dashes
///
/// # Returns
/// Clean 9-character Grid9 code without dashes
///
/// # Example
/// ```rust
/// use grid9::remove_formatting;
/// 
/// let clean = remove_formatting("Q7K-H2B-BYF"); // "Q7KH2BBYF"
/// ```
pub fn remove_formatting(formatted: &str) -> String {
    formatted.chars().filter(|&c| c != '-').collect()
}

/// Gets precision information for coordinates at the given location.
///
/// # Arguments
/// * `latitude` - Latitude in degrees
/// * `longitude` - Longitude in degrees
///
/// # Returns
/// Precision information including lat/lon errors and total error
///
/// # Example
/// ```rust
/// use grid9::get_actual_precision;
/// 
/// let precision = get_actual_precision(40.7128, -74.0060)?;
/// println!("Total precision: {:.1}m", precision.total_error_m);
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn get_actual_precision(latitude: f64, longitude: f64) -> Result<PrecisionInfo> {
    validate_coordinates(latitude, longitude)?;
    
    // Calculate precision based on latitude quantization
    let lat_precision = 180.0 / (1u64 << LAT_BITS) as f64;
    let lat_error_m = lat_precision * 111_320.0;
    
    // Calculate precision based on longitude quantization and latitude
    let lon_precision = 360.0 / (1u64 << LON_BITS) as f64;
    let lon_error_m = lon_precision * 111_320.0 * latitude.to_radians().cos();
    
    // Total error is the diagonal of the error rectangle
    let total_error_m = (lat_error_m * lat_error_m + lon_error_m * lon_error_m).sqrt();
    
    Ok(PrecisionInfo {
        lat_error_m,
        lon_error_m,
        total_error_m,
    })
}

fn validate_coordinates(latitude: f64, longitude: f64) -> Result<()> {
    if !(-90.0..=90.0).contains(&latitude) {
        return Err(Grid9Error::InvalidLatitude(latitude));
    }
    if !(-180.0..=180.0).contains(&longitude) {
        return Err(Grid9Error::InvalidLongitude(longitude));
    }
    Ok(())
}

fn validate_encoded_string(encoded: &str) -> Result<()> {
    if encoded.is_empty() {
        return Err(Grid9Error::EmptyInput);
    }
    
    if encoded.len() != 9 {
        return Err(Grid9Error::InvalidLength(encoded.len()));
    }
    
    for ch in encoded.chars() {
        if !BASE32_ALPHABET.iter().any(|&b| b as char == ch) {
            return Err(Grid9Error::InvalidCharacter(ch));
        }
    }
    
    Ok(())
}

fn haversine_distance(lat1: f64, lon1: f64, lat2: f64, lon2: f64) -> f64 {
    let d_lat = (lat2 - lat1).to_radians();
    let d_lon = (lon2 - lon1).to_radians();
    
    let a = (d_lat / 2.0).sin().powi(2) +
            lat1.to_radians().cos() * lat2.to_radians().cos() *
            (d_lon / 2.0).sin().powi(2);
    
    let c = 2.0 * a.sqrt().atan2((1.0 - a).sqrt());
    
    EARTH_RADIUS_M * c
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_encode_decode_roundtrip() {
        let test_coords = [
            (40.7128, -74.0060), // NYC
            (51.5074, -0.1278),  // London
            (35.6762, 139.6503), // Tokyo
            (0.0, 0.0),          // Equator/Prime Meridian
            (-33.8688, 151.2093), // Sydney
        ];

        for (lat, lon) in test_coords {
            let encoded = encode(lat, lon, false).unwrap();
            let (decoded_lat, decoded_lon) = decode(&encoded).unwrap();
            
            // Should be within precision bounds
            assert!((decoded_lat - lat).abs() < 0.01, 
                "Latitude mismatch: {} vs {}", lat, decoded_lat);
            assert!((decoded_lon - lon).abs() < 0.01,
                "Longitude mismatch: {} vs {}", lon, decoded_lon);
        }
    }

    #[test]
    fn test_human_readable_formatting() {
        let encoded = encode(40.7128, -74.0060, false).unwrap();
        let readable = format_for_humans(&encoded);
        
        assert_eq!(readable.len(), 11);
        assert_eq!(readable.chars().filter(|&c| c == '-').count(), 2);
        
        // Should decode to same coordinates
        let (lat1, lon1) = decode(&encoded).unwrap();
        let (lat2, lon2) = decode(&readable).unwrap();
        
        assert!((lat1 - lat2).abs() < f64::EPSILON);
        assert!((lon1 - lon2).abs() < f64::EPSILON);
    }

    #[test]
    fn test_distance_calculation() {
        let nyc = encode(40.7128, -74.0060, false).unwrap();
        let london = encode(51.5074, -0.1278, false).unwrap();
        
        let distance = calculate_distance(&nyc, &london).unwrap();
        
        // NYC to London is approximately 5,570 km
        assert!(distance > 5_500_000.0 && distance < 5_600_000.0);
    }

    #[test]
    fn test_precision_info() {
        let precision = get_actual_precision(40.7128, -74.0060).unwrap();
        
        assert!(precision.lat_error_m > 0.0);
        assert!(precision.lon_error_m > 0.0);
        assert!(precision.total_error_m > 0.0);
        assert!(precision.total_error_m < 5.0); // Should be under 5 meters
    }

    #[test]
    fn test_validation() {
        // Valid encodings
        assert!(is_valid_encoding("Q7KH2BBYF"));
        assert!(is_valid_encoding("Q7K-H2B-BYF"));
        
        // Invalid encodings
        assert!(!is_valid_encoding(""));
        assert!(!is_valid_encoding("TOOLONG"));
        assert!(!is_valid_encoding("INVALID!"));
        assert!(!is_valid_encoding("Q7KH2BBY")); // Too short
    }

    #[test]
    fn test_edge_cases() {
        // Test boundary coordinates
        let boundary_coords = [
            (90.0, 180.0),   // North Pole, Date Line
            (-90.0, -180.0), // South Pole, Date Line
            (89.9, 179.9),   // Near boundaries
            (-89.9, -179.9), // Near boundaries
        ];

        for (lat, lon) in boundary_coords {
            let encoded = encode(lat, lon, false).unwrap();
            let (decoded_lat, decoded_lon) = decode(&encoded).unwrap();
            
            assert!((decoded_lat - lat).abs() < 0.1);
            assert!((decoded_lon - lon).abs() < 0.1);
        }
    }

    #[test]
    fn test_error_conditions() {
        // Invalid coordinates
        assert!(matches!(encode(91.0, 0.0, false), Err(Grid9Error::InvalidLatitude(_))));
        assert!(matches!(encode(-91.0, 0.0, false), Err(Grid9Error::InvalidLatitude(_))));
        assert!(matches!(encode(0.0, 181.0, false), Err(Grid9Error::InvalidLongitude(_))));
        assert!(matches!(encode(0.0, -181.0, false), Err(Grid9Error::InvalidLongitude(_))));
        
        // Invalid encoded strings
        assert!(matches!(decode(""), Err(Grid9Error::EmptyInput)));
        assert!(matches!(decode("TOOLONG"), Err(Grid9Error::InvalidLength(_))));
        assert!(matches!(decode("INVALID!"), Err(Grid9Error::InvalidCharacter(_))));
    }
}