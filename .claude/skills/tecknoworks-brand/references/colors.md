# Tecknoworks Color System

Complete color specifications for all brand applications.

## Primary Colors

### Dark Gray (Pantone 445 C)
- **CMYK**: 40, 31, 20, 70
- **RGB**: 77, 80, 88
- **HEX**: #515455
- **Use**: Text, secondary elements, monochrome logo

### Deep Purple (Pantone 2755 C)
- **CMYK**: 96, 100, 4, 41
- **RGB**: 54, 47, 102
- **HEX**: #320063
- **Use**: Accent backgrounds, geometric patterns

### Plum (Pantone 683 C)
- **CMYK**: 37, 100, 36, 17
- **RGB**: 149, 50, 99
- **HEX**: #7D1C4B
- **Use**: Accent elements, gradients

### Magenta (Pantone 7424 C)
- **CMYK**: 6, 100, 44, 0
- **RGB**: 225, 48, 104
- **HEX**: #EA2775
- **Use**: Primary brand color, logo airplane, highlights

### Coral (Pantone 170 C)
- **CMYK**: 3, 63, 56, 0
- **RGB**: 237, 129, 111
- **HEX**: #EA8170
- **Use**: Accent, warm gradient elements

## Color Tints

Each primary color can be used at reduced opacity:
- **70%** - For lighter backgrounds
- **50%** - For subtle accents
- **30%** - For very light backgrounds

## Color Usage Guidelines

### Backgrounds
- **White**: Preferred for all documents and presentations
- **Dark backgrounds**: Use Deep Purple or Dark Gray (max 50% black for logo visibility)
- **Gradient backgrounds**: Blend Magenta → Coral → Orange for geometric patterns

### Text Colors
- **Primary text**: Black or Dark Gray (#515455)
- **Headlines**: Deep Purple (#320063) or Magenta (#EA2775)
- **Links/accents**: Magenta (#EA2775)

### Geometric Pattern
The brand uses a triangular mosaic pattern blending:
- Deep Purple
- Plum
- Magenta
- Coral
- Orange tints

### Logo on Backgrounds
- **White background**: Full-color logo (preferred)
- **Light gray (max 50%)**: Full-color logo acceptable
- **Dark backgrounds**: White reverse signature
- **Pink/red/warm colors**: NOT recommended for logo placement

## CSS Variables for Digital

```css
:root {
  --tw-dark-gray: #515455;
  --tw-deep-purple: #320063;
  --tw-plum: #7D1C4B;
  --tw-magenta: #EA2775;
  --tw-coral: #EA8170;
  
  /* Text colors */
  --tw-text-primary: #515455;
  --tw-text-headline: #660033;
  --tw-text-subtitle: #CC0066;
}
```

## PowerPoint Color Codes

For PowerPoint title/subtitle text:
- **Title**: RGB 102, 0, 51 (dark magenta)
- **Subtitle**: RGB 204, 0, 102 (bright magenta)
- **Body**: Black
