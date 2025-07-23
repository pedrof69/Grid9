//! Grid9 Rust Implementation Demo
//! 
//! This example demonstrates the key features of the Grid9 coordinate
//! compression system in Rust.

use grid9::*;

fn main() -> Result<()> {
    println!("=== Grid9 Rust Implementation Demo ===\n");
    
    // Test coordinates: NYC, London, Tokyo
    let test_coords = [
        (40.7128, -74.0060, "New York"),
        (51.5074, -0.1278, "London"),
        (35.6762, 139.6503, "Tokyo"),
    ];
    
    println!("=== Basic Encoding/Decoding ===");
    for (lat, lon, city) in &test_coords {
        // Encode
        let compact = encode(*lat, *lon, false)?;
        let readable = encode(*lat, *lon, true)?;
        
        // Decode
        let (decoded_lat, decoded_lon) = decode(&compact)?;
        
        // Precision info
        let precision = get_actual_precision(*lat, *lon)?;
        
        println!("{}:", city);
        println!("  Original: ({:.6}, {:.6})", lat, lon);
        println!("  Compact:  {}", compact);
        println!("  Readable: {}", readable);
        println!("  Decoded:  ({:.6}, {:.6})", decoded_lat, decoded_lon);
        println!("  Precision: {:.1}m total", precision.total_error_m);
        println!();
    }
    
    println!("=== Distance Calculation ===");
    let nyc_code = encode(40.7128, -74.0060, false)?;
    let london_code = encode(51.5074, -0.1278, false)?;
    
    let distance = calculate_distance(&nyc_code, &london_code)?;
    println!("Distance NYC to London: {:.0} meters\n", distance);
    
    println!("=== Batch Operations ===");
    let coordinates: Vec<Coordinate> = test_coords
        .iter()
        .map(|(lat, lon, _)| Coordinate::new(*lat, *lon))
        .collect();
    
    let encoded_batch = batch_encode(&coordinates, false)?;
    let decoded_batch = batch_decode(&encoded_batch)?;
    
    println!("Batch encoded {} coordinates:", encoded_batch.len());
    for (i, code) in encoded_batch.iter().enumerate() {
        println!("  {}: {}", test_coords[i].2, code);
    }
    println!();
    
    println!("=== Validation ===");
    let test_strings = [
        ("Q7KH2BBYF", "Valid"),
        ("Q7K-H2B-BYF", "Valid with dashes"),
        ("INVALID123", "Invalid"),
        ("TOOLONGSTRING", "Invalid length"),
    ];
    
    for (test_str, description) in &test_strings {
        let is_valid = is_valid_encoding(test_str);
        println!("\"{}\" is {} ({})", test_str, if is_valid { "valid" } else { "invalid" }, description);
    }
    println!();
    
    println!("=== Nearby Search ===");
    let center_lat = 40.7128; // NYC
    let center_lon = -74.0060;
    let radius = 1000.0; // 1km
    
    let nearby = find_nearby(center_lat, center_lon, radius, 5)?;
    println!("Found {} points within {}m of NYC:", nearby.len(), radius);
    for code in &nearby {
        let (lat, lon) = decode(code)?;
        println!("  {} -> ({:.6}, {:.6})", code, lat, lon);
    }
    println!();
    
    println!("=== Spatial Analysis ===");
    let bounding_box = get_bounding_box(&coordinates)?;
    let center_point = get_center_point(&coordinates)?;
    
    println!("Bounding box:");
    println!("  Min: ({:.6}, {:.6})", bounding_box.min_lat, bounding_box.min_lon);
    println!("  Max: ({:.6}, {:.6})", bounding_box.max_lat, bounding_box.max_lon);
    println!("Center point: ({:.6}, {:.6})", center_point.lat, center_point.lon);
    println!();
    
    println!("=== Grouping by Grid9 Codes ===");
    let grouped = group_by_grid9(&coordinates, false)?;
    println!("Coordinates grouped into {} unique Grid9 codes:", grouped.len());
    for (code, coords) in grouped {
        println!("  {}: {} coordinate(s)", code, coords.len());
    }
    println!();
    
    println!("=== Format Conversion ===");
    let test_code = "Q7KH2BBYF";
    let formatted = format_for_humans(test_code);
    let unformatted = remove_formatting(&formatted);
    
    println!("Original:    {}", test_code);
    println!("Formatted:   {}", formatted);
    println!("Unformatted: {}", unformatted);
    println!();
    
    println!("=== Precision Comparison ===");
    println!("System      | Characters | Global Precision | Example");
    println!("------------|------------|------------------|----------");
    println!("Grid9       | 9          | 2.4-3.5m        | Q7KH2BBYF");
    println!("What3Words  | 19+        | 3m               | filled.count.soap");
    println!("Plus Codes  | 11+        | 2-14m            | 87G8Q23F+GF");
    println!("Geohash     | 12         | 1-18m            | dr5regw3pg6");
    println!();
    
    println!("Demo completed successfully!");
    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_demo_runs() {
        // This test ensures the demo code runs without panicking
        main().unwrap();
    }
}