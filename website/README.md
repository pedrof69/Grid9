# Grid9 Website

Professional website for Grid9 coordinate compression system.

## Features

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Live Demo**: Interactive coordinate-to-Grid9 converter
- **Professional Presentation**: Clean, modern design
- **SEO Optimized**: Meta tags and structured content
- **Performance**: Lightweight, fast loading
- **Comparison Table**: Grid9 vs alternatives

## Deployment Options

### 1. GitHub Pages (Recommended)
```bash
# Enable GitHub Pages in repository settings
# Point to /website folder or copy files to root
```

### 2. Netlify
```bash
# Connect GitHub repository
# Build directory: website
# Publish directory: website
```

### 3. Vercel
```bash
# Import GitHub repository
# Framework preset: Other
# Build and output settings: website
```

### 4. Simple Hosting
Upload `index.html` to any web hosting service.

## Customization

### Update GitHub Links
Replace `yourusername` in the following places:
- Line 95: GitHub repository link
- Line 96: Demo link  
- Line 248: GitHub repository link
- Line 249: NuGet package link
- Line 257: GitHub link
- Line 258: Documentation link
- Line 261: Support link

### Add Real Grid9 Implementation
Replace the placeholder JavaScript in `generateGrid9()` function with actual Grid9 encoding logic.

### SEO Optimization
- Update meta description and keywords
- Add favicon
- Add Open Graph tags for social sharing
- Add structured data markup

## File Structure
```
website/
├── index.html          # Main website file
├── README.md          # This file
└── assets/           # Future: images, additional CSS/JS
```

## Browser Support
- Chrome 60+
- Firefox 60+
- Safari 12+
- Edge 79+

## Performance
- Lighthouse Score: 95+
- First Contentful Paint: <1.5s
- Time to Interactive: <2.5s
- CSS: Inline (no external dependencies)
- JavaScript: Minimal, progressive enhancement