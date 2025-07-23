//! # Grid9 - Precision Coordinate Compression
//!
//! A high-performance Rust implementation of the Grid9 coordinate compression system
//! featuring **9-character Grid9 codes** with **uniform 3-meter precision globally**.
//!
//! ## Features
//!
//! - **9 characters** - Optimal length for 3m global precision
//! - **Human-readable** - Optional XXX-XXX-XXX formatting with dashes
//! - **Global coverage** - Works everywhere on Earth (including oceans)
//! - **Uniform precision** - 2.4-3.5m accuracy consistently worldwide
//! - **High performance** - Zero-allocation encoding/decoding
//! - **Rust safety** - Memory-safe with comprehensive error handling
//!
//! ## Quick Start
//!
//! ```rust
//! use grid9::{encode, decode, calculate_distance};
//!
//! // Encode coordinates
//! let code = encode(40.7128, -74.0060, false)?; // "Q7KH2BBYF" - NYC
//! let readable = encode(40.7128, -74.0060, true)?; // "Q7K-H2B-BYF" - Same precision
//!
//! // Decode coordinates  
//! let (lat, lon) = decode(&code)?; // (40.712779, -74.005988)
//!
//! // Calculate distance between codes
//! let london_code = encode(51.5074, -0.1278, false)?;
//! let distance = calculate_distance(&code, &london_code)?; // ~5,570km
//! ```

pub mod coordinate_operations;
pub mod uniform_precision_compressor;
pub mod test_simple;

pub use coordinate_operations::*;
pub use uniform_precision_compressor::*;

/// Grid9 error types
#[derive(Debug, Clone, PartialEq)]
pub enum Grid9Error {
    /// Invalid latitude (must be between -90 and 90)
    InvalidLatitude(f64),
    /// Invalid longitude (must be between -180 and 180) 
    InvalidLongitude(f64),
    /// Invalid encoded string length (must be 9 characters after removing dashes)
    InvalidLength(usize),
    /// Invalid character in encoded string
    InvalidCharacter(char),
    /// Empty input
    EmptyInput,
}

impl std::fmt::Display for Grid9Error {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Grid9Error::InvalidLatitude(lat) => {
                write!(f, "Invalid latitude: {} (must be between -90 and 90)", lat)
            }
            Grid9Error::InvalidLongitude(lon) => {
                write!(f, "Invalid longitude: {} (must be between -180 and 180)", lon)
            }
            Grid9Error::InvalidLength(len) => {
                write!(f, "Invalid encoded string length: {} (must be 9 characters)", len)
            }
            Grid9Error::InvalidCharacter(ch) => {
                write!(f, "Invalid character in encoded string: '{}'", ch)
            }
            Grid9Error::EmptyInput => {
                write!(f, "Empty input string")
            }
        }
    }
}

impl std::error::Error for Grid9Error {}

/// Result type for Grid9 operations
pub type Result<T> = std::result::Result<T, Grid9Error>;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_basic_encode_decode() {
        let lat = 40.7128;
        let lon = -74.0060;
        
        let encoded = encode(lat, lon, false).unwrap();
        assert_eq!(encoded.len(), 9);
        
        let (decoded_lat, decoded_lon) = decode(&encoded).unwrap();
        assert!((decoded_lat - lat).abs() < 0.01);
        assert!((decoded_lon - lon).abs() < 0.01);
    }

    #[test]
    fn test_human_readable_format() {
        let encoded = encode(40.7128, -74.0060, true).unwrap();
        assert_eq!(encoded.len(), 11); // 9 chars + 2 dashes
        assert!(encoded.contains('-'));
        
        // Should decode the same as compact format
        let compact = encode(40.7128, -74.0060, false).unwrap();
        let (lat1, lon1) = decode(&encoded).unwrap();
        let (lat2, lon2) = decode(&compact).unwrap();
        
        assert!((lat1 - lat2).abs() < f64::EPSILON);
        assert!((lon1 - lon2).abs() < f64::EPSILON);
    }

    #[test]
    fn test_validation() {
        assert!(is_valid_encoding("Q7KH2BBYF"));
        assert!(is_valid_encoding("Q7K-H2B-BYF"));
        assert!(!is_valid_encoding("INVALID"));
        assert!(!is_valid_encoding("Q7KH2BBYFI")); // Too long
    }

    #[test]
    fn test_distance_calculation() {
        let nyc = encode(40.7128, -74.0060, false).unwrap();
        let london = encode(51.5074, -0.1278, false).unwrap();
        
        let distance = calculate_distance(&nyc, &london).unwrap();
        assert!(distance > 5_500_000.0 && distance < 5_600_000.0);
    }

    #[test]
    fn test_error_handling() {
        // Invalid coordinates
        assert!(matches!(encode(91.0, 0.0, false), Err(Grid9Error::InvalidLatitude(_))));
        assert!(matches!(encode(0.0, 181.0, false), Err(Grid9Error::InvalidLongitude(_))));
        
        // Invalid encoded strings
        assert!(matches!(decode(""), Err(Grid9Error::EmptyInput)));
        assert!(matches!(decode("TOOLONG"), Err(Grid9Error::InvalidLength(_))));
        assert!(matches!(decode("INVALID!"), Err(Grid9Error::InvalidCharacter(_))));
    }
}