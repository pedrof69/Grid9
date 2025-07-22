# Reddit Post for Grid9 Launch

## Title Options (choose one based on subreddit):

### For r/programming:
"Grid9: Open-source 9-character coordinate compression with 3-meter precision"

### For r/opensource:
"[Open Source] Grid9 - I built a coordinate compression system that matches what3words precision in 53% fewer characters"

### For r/selfhosted:
"Grid9 - Self-hostable coordinate compression alternative to what3words (9 chars vs 19+)"

---

## Main Post:

Hey everyone! I'm excited to share Grid9, an open-source coordinate compression system I've been working on.

**What is Grid9?**
Grid9 compresses GPS coordinates into just 9 characters while maintaining uniform 3-meter precision globally - the same accuracy as what3words but 53% shorter.

**Key Features:**
- **9-character codes**: `Q7KH2BBYF` instead of `40.7128, -74.0060`
- **3-meter precision**: Accurate enough for autonomous vehicles and precision agriculture
- **Human-readable option**: `Q7K-H2B-BYF` format for easier communication
- **High performance**: 6+ million operations/second
- **No dependencies**: Pure coordinate math, no external services needed
- **Free for non-commercial use**: MIT-style license for personal projects

**Why I built this:**
The push for autonomous vehicles and precision applications demands compact, accurate location encoding. Traditional lat/lon is too verbose for bandwidth-constrained systems, and what3words, while brilliant, uses 19+ characters. Grid9 achieves the same precision in just 9 characters.

**Technical approach:**
Grid9 uses uniform coordinate quantization - direct latitude and longitude quantization in degree space. This simple approach achieves consistent global precision without complex projections. The result fits perfectly into 45 bits (9 × 5-bit base32 characters).

**Example:**
```
New York: 40.7128, -74.0060 → Q7KH2BBYF
London:   51.5074, -0.1278  → S50MBZX2Y  
Tokyo:    35.6762, 139.6503 → PAYMZ39T7
```

**Get started:**
- GitHub: https://github.com/pedrof69/Grid9
- Demo: https://pedrof69.github.io/Grid9/
- NuGet: `dotnet add package Grid9`

**Commercial licensing:** Available at grid9@ukdataservices.co.uk

I'd love to hear your feedback and answer any questions. The code is production-ready with comprehensive tests, and I'm actively maintaining it.

---

## Subreddit-specific additions:

### For r/programming:
Add: "The codebase is in C# with zero dependencies. PRs welcome for ports to other languages!"

### For r/selfdriving:
Add: "Designed specifically for autonomous vehicle waypoint storage and real-time navigation data."

### For r/gis:
Add: "Grid9 provides uniform precision globally including oceans and polar regions - 2.4-3.5m accuracy everywhere on Earth."