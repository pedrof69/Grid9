using OptimalCoordinateCompression;

namespace Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("GRID9 COORDINATE COMPRESSION DEMO");
            Console.WriteLine("=================================");
            Console.WriteLine("what3words precision with 9-character Grid9 codes using hybrid meter-based quantization\n");

            // Famous landmarks demonstration
            DemoFamousLandmarks();
            
            // Human-readable formatting demo
            DemoHumanReadableFormatting();
            
            // Interactive demo
            InteractiveDemo();
        }

        static void DemoFamousLandmarks()
        {
            Console.WriteLine("FAMOUS LANDMARKS GRID9 COMPRESSION");
            Console.WriteLine("==================================");
            
            var landmarks = new[]
            {
                ("New York City", 40.7128, -74.0060),
                ("London", 51.5074, -0.1278),
                ("Tokyo", 35.6762, 138.6503),
                ("Sydney", -33.8688, 151.2083),
                ("Paris", 48.8566, 2.3522)
            };

            foreach (var (name, lat, lon) in landmarks)
            {
                string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                var (latErrorM, lonErrorM, totalErrorM) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);
                
                Console.WriteLine($"{name,-15}: {lat,8:F4}, {lon,8:F4} → {encoded} (error: {totalErrorM:F1}m)");
            }
            Console.WriteLine();
        }

        static void DemoHumanReadableFormatting()
        {
            Console.WriteLine("HUMAN-READABLE FORMATTING");
            Console.WriteLine("=========================");
            Console.WriteLine("Grid9 codes can be formatted with dashes for better readability:\n");
            
            var examples = new[]
            {
                ("Statue of Liberty", 40.6892, -74.0445),
                ("Big Ben", 51.5007, -0.1246),
                ("Sydney Opera House", -33.8568, 151.2153)
            };

            foreach (var (name, lat, lon) in examples)
            {
                string compact = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: false);
                string readable = MeterBasedCoordinateCompressor.Encode(lat, lon, humanReadable: true);
                
                Console.WriteLine($"{name}:");
                Console.WriteLine($"  Compact:  {compact}");
                Console.WriteLine($"  Readable: {readable}");
                
                // Demonstrate both formats decode to same result
                var (lat1, lon1) = MeterBasedCoordinateCompressor.Decode(compact);
                var (lat2, lon2) = MeterBasedCoordinateCompressor.Decode(readable);
                
                Console.WriteLine($"  Both decode to: ({lat1:F6}, {lon1:F6})");
                Console.WriteLine();
            }
        }

        static void InteractiveDemo()
        {
            Console.WriteLine("INTERACTIVE DEMO");
            Console.WriteLine("================");
            Console.WriteLine("Enter coordinates to compress (or 'quit' to exit):");
            
            while (true)
            {
                Console.Write("\nLatitude: ");
                string? latInput = Console.ReadLine();
                if (latInput?.ToLower() == "quit") break;
                
                Console.Write("Longitude: ");
                string? lonInput = Console.ReadLine();
                if (lonInput?.ToLower() == "quit") break;
                
                if (double.TryParse(latInput, out double lat) && 
                    double.TryParse(lonInput, out double lon))
                {
                    try
                    {
                        string encoded = MeterBasedCoordinateCompressor.Encode(lat, lon);
                        var (decodedLat, decodedLon) = MeterBasedCoordinateCompressor.Decode(encoded);
                        var (latErrorM, lonErrorM, totalErrorM) = MeterBasedCoordinateCompressor.GetActualPrecision(lat, lon);
                        
                        Console.WriteLine($"\nOriginal:  ({lat:F6}, {lon:F6})");
                        Console.WriteLine($"Encoded:   {encoded}");
                        Console.WriteLine($"Decoded:   ({decodedLat:F6}, {decodedLon:F6})");
                        Console.WriteLine($"Error:     {totalErrorM:F1}m");
                        Console.WriteLine($"Status:    {(totalErrorM <= 3.0 ? "✓ PASS" : "✗ FAIL")} (what3words standard)");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid coordinates. Please enter valid numbers.");
                }
            }
        }
    }
}