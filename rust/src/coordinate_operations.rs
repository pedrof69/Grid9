//! High-performance coordinate operations with batch processing capabilities

use crate::{decode, encode, calculate_distance, Grid9Error, Result};

/// A coordinate point with latitude and longitude
#[derive(Debug, Clone, PartialEq)]
#[cfg_attr(feature = "serde", derive(serde::Serialize, serde::Deserialize))]
pub struct Coordinate {
    pub lat: f64,
    pub lon: f64,
}

impl Coordinate {
    /// Creates a new coordinate
    pub fn new(lat: f64, lon: f64) -> Self {
        Self { lat, lon }
    }
}

/// A bounding box defined by minimum and maximum coordinates
#[derive(Debug, Clone, PartialEq)]
#[cfg_attr(feature = "serde", derive(serde::Serialize, serde::Deserialize))]
pub struct BoundingBox {
    pub min_lat: f64,
    pub max_lat: f64,
    pub min_lon: f64,
    pub max_lon: f64,
}

impl BoundingBox {
    /// Creates a new bounding box
    pub fn new(min_lat: f64, max_lat: f64, min_lon: f64, max_lon: f64) -> Self {
        Self {
            min_lat,
            max_lat,
            min_lon,
            max_lon,
        }
    }
}

/// Batch encodes multiple coordinate pairs for high-throughput scenarios.
///
/// # Arguments
/// * `coordinates` - Vector of coordinates to encode
/// * `human_readable` - If true, returns codes in XXX-XXX-XXX format
///
/// # Returns
/// Vector of encoded Grid9 strings
///
/// # Example
/// ```rust
/// use grid9::{batch_encode, Coordinate};
/// 
/// let coords = vec![
///     Coordinate::new(40.7128, -74.0060), // NYC
///     Coordinate::new(51.5074, -0.1278),  // London
/// ];
/// let encoded = batch_encode(&coords, false)?;
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn batch_encode(coordinates: &[Coordinate], human_readable: bool) -> Result<Vec<String>> {
    coordinates
        .iter()
        .map(|coord| encode(coord.lat, coord.lon, human_readable))
        .collect()
}

/// Batch decodes multiple encoded strings for high-throughput scenarios.
///
/// # Arguments
/// * `encoded` - Vector of encoded Grid9 strings
///
/// # Returns
/// Vector of coordinate objects
///
/// # Example
/// ```rust
/// use grid9::{batch_decode, encode, Coordinate};
/// 
/// let coords = vec![
///     encode(40.7128, -74.0060, false)?,
///     encode(51.5074, -0.1278, false)?,
/// ];
/// let decoded = batch_decode(&coords)?;
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn batch_decode(encoded: &[String]) -> Result<Vec<Coordinate>> {
    encoded
        .iter()
        .map(|enc| {
            let (lat, lon) = decode(enc)?;
            Ok(Coordinate::new(lat, lon))
        })
        .collect()
}

/// Finds all coordinates within a specified radius (in meters) of a center point.
/// Returns encoded strings of nearby coordinates.
///
/// # Arguments
/// * `center_lat` - Center latitude in degrees
/// * `center_lon` - Center longitude in degrees
/// * `radius_meters` - Search radius in meters
/// * `max_results` - Maximum number of results to return
///
/// # Returns
/// Vector of encoded Grid9 strings within the radius
///
/// # Example
/// ```rust
/// use grid9::find_nearby;
/// 
/// let nearby = find_nearby(40.7128, -74.0060, 1000.0, 10)?; // 1km around NYC
/// println!("Found {} nearby points", nearby.len());
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn find_nearby(
    center_lat: f64,
    center_lon: f64,
    radius_meters: f64,
    max_results: usize,
) -> Result<Vec<String>> {
    if radius_meters <= 0.0 {
        return Err(Grid9Error::InvalidLatitude(radius_meters)); // Reuse error type
    }
    
    let mut results = Vec::new();
    let center_encoded = encode(center_lat, center_lon, false)?;
    
    // Calculate approximate grid search bounds
    let lat_delta = radius_meters / 111_320.0; // Rough conversion
    let lon_delta = radius_meters / (111_320.0 * (center_lat * std::f64::consts::PI / 180.0).cos());
    
    let min_lat = (center_lat - lat_delta).max(-80.0);
    let max_lat = (center_lat + lat_delta).min(80.0);
    let min_lon = (center_lon - lon_delta).max(-180.0);
    let max_lon = (center_lon + lon_delta).min(180.0);
    
    // Grid search with 3-meter steps
    let lat_step = 3.0 / 111_320.0;
    let lon_step = 3.0 / 111_320.0;
    
    let mut lat = min_lat;
    while lat <= max_lat && results.len() < max_results {
        let mut lon = min_lon;
        while lon <= max_lon && results.len() < max_results {
            if let Ok(encoded) = encode(lat, lon, false) {
                if let Ok(distance) = calculate_distance(&center_encoded, &encoded) {
                    if distance <= radius_meters {
                        results.push(encoded);
                    }
                }
            }
            lon += lon_step;
        }
        lat += lat_step;
    }
    
    Ok(results)
}

/// Calculates the bounding box that contains all given coordinates.
///
/// # Arguments
/// * `coordinates` - Vector of coordinates
///
/// # Returns
/// Bounding box with min/max lat/lon values
///
/// # Example
/// ```rust
/// use grid9::{get_bounding_box, Coordinate};
/// 
/// let coords = vec![
///     Coordinate::new(40.7128, -74.0060), // NYC
///     Coordinate::new(51.5074, -0.1278),  // London
/// ];
/// let bbox = get_bounding_box(&coords)?;
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn get_bounding_box(coordinates: &[Coordinate]) -> Result<BoundingBox> {
    if coordinates.is_empty() {
        return Err(Grid9Error::EmptyInput);
    }
    
    let first = &coordinates[0];
    let mut min_lat = first.lat;
    let mut max_lat = first.lat;
    let mut min_lon = first.lon;
    let mut max_lon = first.lon;
    
    for coord in coordinates.iter().skip(1) {
        min_lat = min_lat.min(coord.lat);
        max_lat = max_lat.max(coord.lat);
        min_lon = min_lon.min(coord.lon);
        max_lon = max_lon.max(coord.lon);
    }
    
    Ok(BoundingBox::new(min_lat, max_lat, min_lon, max_lon))
}

/// Calculates the center point of a set of coordinates.
///
/// # Arguments
/// * `coordinates` - Vector of coordinates
///
/// # Returns
/// Center coordinate (arithmetic mean)
///
/// # Example
/// ```rust
/// use grid9::{get_center_point, Coordinate};
/// 
/// let coords = vec![
///     Coordinate::new(40.7128, -74.0060), // NYC
///     Coordinate::new(51.5074, -0.1278),  // London
/// ];
/// let center = get_center_point(&coords)?;
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn get_center_point(coordinates: &[Coordinate]) -> Result<Coordinate> {
    if coordinates.is_empty() {
        return Err(Grid9Error::EmptyInput);
    }
    
    let total_lat: f64 = coordinates.iter().map(|c| c.lat).sum();
    let total_lon: f64 = coordinates.iter().map(|c| c.lon).sum();
    
    Ok(Coordinate::new(
        total_lat / coordinates.len() as f64,
        total_lon / coordinates.len() as f64,
    ))
}

/// Groups coordinates by their Grid9 codes for spatial indexing.
///
/// # Arguments
/// * `coordinates` - Vector of coordinates
/// * `human_readable` - If true, uses XXX-XXX-XXX format for keys
///
/// # Returns
/// HashMap mapping Grid9 codes to vectors of coordinates with that code
///
/// # Example
/// ```rust
/// use grid9::{group_by_grid9, Coordinate};
/// 
/// let coords = vec![
///     Coordinate::new(40.7128, -74.0060),
///     Coordinate::new(40.7130, -74.0062), // Very close to first
/// ];
/// let grouped = group_by_grid9(&coords, false)?;
/// # Ok::<(), grid9::Grid9Error>(())
/// ```
pub fn group_by_grid9(
    coordinates: &[Coordinate],
    human_readable: bool,
) -> Result<std::collections::HashMap<String, Vec<Coordinate>>> {
    use std::collections::HashMap;
    
    let mut groups: HashMap<String, Vec<Coordinate>> = HashMap::new();
    
    for coord in coordinates {
        let code = encode(coord.lat, coord.lon, human_readable)?;
        groups.entry(code).or_insert_with(Vec::new).push(coord.clone());
    }
    
    Ok(groups)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_batch_encode_decode() {
        let coordinates = vec![
            Coordinate::new(40.7128, -74.0060), // NYC
            Coordinate::new(51.5074, -0.1278),  // London
            Coordinate::new(35.6762, 139.6503), // Tokyo
        ];
        
        let encoded = batch_encode(&coordinates, false).unwrap();
        assert_eq!(encoded.len(), 3);
        
        let decoded = batch_decode(&encoded).unwrap();
        assert_eq!(decoded.len(), 3);
        
        for (orig, decoded) in coordinates.iter().zip(decoded.iter()) {
            assert!((orig.lat - decoded.lat).abs() < 0.01);
            assert!((orig.lon - decoded.lon).abs() < 0.01);
        }
    }

    #[test]
    fn test_bounding_box() {
        let coordinates = vec![
            Coordinate::new(40.0, -75.0),
            Coordinate::new(41.0, -73.0),
            Coordinate::new(39.0, -76.0),
        ];
        
        let bbox = get_bounding_box(&coordinates).unwrap();
        
        assert!((bbox.min_lat - 39.0).abs() < f64::EPSILON);
        assert!((bbox.max_lat - 41.0).abs() < f64::EPSILON);
        assert!((bbox.min_lon - (-76.0)).abs() < f64::EPSILON);
        assert!((bbox.max_lon - (-73.0)).abs() < f64::EPSILON);
    }

    #[test]
    fn test_center_point() {
        let coordinates = vec![
            Coordinate::new(40.0, -75.0),
            Coordinate::new(42.0, -73.0),
        ];
        
        let center = get_center_point(&coordinates).unwrap();
        
        assert!((center.lat - 41.0).abs() < f64::EPSILON);
        assert!((center.lon - (-74.0)).abs() < f64::EPSILON);
    }

    #[test]
    fn test_find_nearby() {
        let nearby = find_nearby(40.7128, -74.0060, 1000.0, 5).unwrap();
        assert!(!nearby.is_empty());
        assert!(nearby.len() <= 5);
        
        // All results should be valid Grid9 codes
        for code in nearby {
            assert!(crate::is_valid_encoding(&code));
        }
    }

    #[test]
    fn test_group_by_grid9() {
        let coordinates = vec![
            Coordinate::new(40.7128, -74.0060),
            Coordinate::new(40.7130, -74.0062), // Very close
            Coordinate::new(51.5074, -0.1278),  // London - different code
        ];
        
        let grouped = group_by_grid9(&coordinates, false).unwrap();
        
        // Should have at least one group, possibly two if the close points have different codes
        assert!(!grouped.is_empty());
        
        // Total coordinates should match
        let total_coords: usize = grouped.values().map(|v| v.len()).sum();
        assert_eq!(total_coords, coordinates.len());
    }

    #[test]
    fn test_empty_input_errors() {
        let empty_coords = vec![];
        
        assert!(matches!(get_bounding_box(&empty_coords), Err(Grid9Error::EmptyInput)));
        assert!(matches!(get_center_point(&empty_coords), Err(Grid9Error::EmptyInput)));
    }
}