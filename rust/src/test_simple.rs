//! Simple test runner for Grid9 Rust implementation

use crate::*;

pub fn run_all_tests() -> Result<()> {
    println!("=== Grid9 Rust Implementation Tests ===");
    
    test_basic_functionality()?;
    test_human_readable()?;
    test_validation()?;
    test_distance()?;
    test_batch_operations()?;
    
    println!("\n🎉 All tests passed!");
    Ok(())
}

fn test_basic_functionality() -> Result<()> {
    println!("Testing basic encode/decode...");
    
    // Test NYC coordinates
    let lat = 40.7128;
    let lon = -74.0060;
    
    let encoded = encode(lat, lon, false)?;
    println!("Encoded {}, {} to: {}", lat, lon, encoded);
    
    let (decoded_lat, decoded_lon) = decode(&encoded)?;
    println!("Decoded back to: {}, {}", decoded_lat, decoded_lon);
    
    // Should be within reasonable precision
    assert!((decoded_lat - lat).abs() < 0.01);
    assert!((decoded_lon - lon).abs() < 0.01);
    
    println!("✓ Basic encode/decode test passed");
    Ok(())
}

fn test_human_readable() -> Result<()> {
    println!("\nTesting human-readable format...");
    
    let compact = encode(40.7128, -74.0060, false)?;
    let readable = encode(40.7128, -74.0060, true)?;
    
    println!("Compact: {}", compact);
    println!("Readable: {}", readable);
    
    // Should have dashes
    assert!(readable.contains('-'));
    assert_eq!(readable.len(), 11); // 9 chars + 2 dashes
    
    // Should decode to same coordinates
    let (lat1, lon1) = decode(&compact)?;
    let (lat2, lon2) = decode(&readable)?;
    
    assert!((lat1 - lat2).abs() < f64::EPSILON);
    assert!((lon1 - lon2).abs() < f64::EPSILON);
    
    println!("✓ Human-readable format test passed");
    Ok(())
}

fn test_validation() -> Result<()> {
    println!("\nTesting validation...");
    
    assert!(is_valid_encoding("Q7KH2BBYF"));
    assert!(is_valid_encoding("Q7K-H2B-BYF"));
    assert!(!is_valid_encoding("INVALID"));
    assert!(!is_valid_encoding("TOOLONG"));
    
    println!("✓ Validation test passed");
    Ok(())
}

fn test_distance() -> Result<()> {
    println!("\nTesting distance calculation...");
    
    let nyc = encode(40.7128, -74.0060, false)?;
    let london = encode(51.5074, -0.1278, false)?;
    
    let distance = calculate_distance(&nyc, &london)?;
    println!("Distance NYC to London: {} meters", distance);
    
    // Should be approximately 5,570 km
    assert!(distance > 5_500_000.0 && distance < 5_600_000.0);
    
    println!("✓ Distance calculation test passed");
    Ok(())
}

fn test_batch_operations() -> Result<()> {
    println!("\nTesting batch operations...");
    
    let coords = vec![
        Coordinate::new(40.7128, -74.0060), // NYC
        Coordinate::new(51.5074, -0.1278),  // London
        Coordinate::new(35.6762, 139.6503), // Tokyo
    ];
    
    let encoded = batch_encode(&coords, false)?;
    let decoded = batch_decode(&encoded)?;
    
    assert_eq!(encoded.len(), 3);
    assert_eq!(decoded.len(), 3);
    
    for (orig, dec) in coords.iter().zip(decoded.iter()) {
        assert!((orig.lat - dec.lat).abs() < 0.01);
        assert!((orig.lon - dec.lon).abs() < 0.01);
    }
    
    println!("✓ Batch operations test passed");
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn run_simple_tests() {
        run_all_tests().unwrap();
    }
}