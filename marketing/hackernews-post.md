# Hacker News Post for Grid9 Launch

## Title:
Show HN: Grid9 – 9-character coordinate compression with 3m precision

## Post:

Hi HN! I've built Grid9, an open-source coordinate compression system that achieves what3words-level precision (3 meters) in just 9 characters instead of 19+.

The motivation came from working with autonomous vehicle systems where bandwidth is precious and coordinate transmission is constant. Traditional lat/lon pairs are verbose (often 20+ characters), and while what3words is brilliant, even their 3-word format is lengthy for high-frequency telemetry.

**Technical approach:**
Grid9 uses uniform coordinate quantization - direct quantization in degree space with optimized bit allocation (22-bit latitude, 23-bit longitude). This simple approach achieves consistent precision globally without complex projections or circular dependencies. The result packs into exactly 45 bits, encoding perfectly as 9 base32 characters.

**Performance:**
- Encoding: 6.4M ops/second
- Decoding: 7.0M ops/second  
- Memory: 32 bytes per operation
- Written in C# with zero dependencies

**Example conversions:**
```
40.7128, -74.0060  →  Q7KH2BBYF  (NYC)
51.5074, -0.1278   →  S50MBZX2Y  (London)
35.6762, 139.6503  →  PAYMZ39T7  (Tokyo)
```

**Human-readable format:**
Grid9 also supports XXX-XXX-XXX formatting for operations teams: `Q7K-H2B-BYF`

**Trade-offs:**
- Fixed precision: Always 2.4-3.5m globally (vs variable precision systems)
- Non-memorable: Unlike what3words' dictionary approach
- Uniform coverage: Same precision everywhere (cities, rural, oceans, poles)

**Use cases:**
Originally built for autonomous vehicles, but applicable to drone operations, precision agriculture, IoT sensors, emergency response, and any system needing compact location encoding.

GitHub: https://github.com/pedrof69/Grid9
Live demo: https://pedrof69.github.io/Grid9/

The code is MIT-licensed for non-commercial use, with commercial licensing available. I'm actively maintaining it and would love feedback from the HN community!

---

## Tips for HN posting:

1. **Best time**: Weekday mornings PST (9-11 AM)
2. **Be ready**: Monitor comments closely for first 2 hours
3. **Technical depth**: HN appreciates technical details - be ready to discuss the algorithm
4. **Humility**: Acknowledge limitations and trade-offs upfront
5. **Engage**: Respond thoughtfully to technical questions

## Potential HN questions to prepare for:

1. "Why not use S2 cells/H3/Geohash?"
   → Grid9 optimizes for uniform 3m precision globally in exactly 9 characters. S2/H3 are hierarchical with variable precision by region.

2. "Why C# instead of Rust/Go/C?"
   → Built for .NET ecosystem initially. The algorithm is simple enough to port - PRs welcome!

3. "What about the what3words patents?"
   → Grid9 uses coordinate quantization, not word encoding. Completely different technical approach.

4. "Does it work in oceans/polar regions?"
   → Yes! Grid9 provides uniform global coverage including oceans and polar regions with consistent precision.

5. "How does it handle edge cases at poles?"
   → Special handling for latitudes >89° where longitude becomes meaningless.