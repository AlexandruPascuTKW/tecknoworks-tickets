# Tecknoworks Typography

Typography reflects the dynamic yet approachable personality of the Tecknoworks brand.

## Primary Typeface

### Nunito (Google Fonts)
- **Source**: https://fonts.google.com/specimen/Nunito
- **License**: Open Font License (free, embedding allowed)
- **Character**: Modern, friendly, rounded, approachable
- **Weights**: Regular (400), Bold (700)

### Arial (Fallback)
- **Use when**: Nunito is unavailable
- **Applications**: System fonts, email, legacy documents

## Font Installation

1. Download from [Google Fonts](https://fonts.google.com/specimen/Nunito)
2. Install system-wide for use in Office applications
3. For web: `@import url('https://fonts.googleapis.com/css2?family=Nunito:wght@400;700&display=swap');`

## Font Embedding (Critical for Documents)

**Word documents**: File → Options → Save → ✓ "Embed fonts in the file"
**PowerPoint**: File → Options → Save → ✓ "Embed fonts in the file"
**PDF export**: Fonts are automatically embedded

Without embedding, recipients without Nunito installed will see Arial substitution.

## Typography Hierarchy

### Presentations (PowerPoint)

| Element | Font | Size | Color |
|---------|------|------|-------|
| Main title | Nunito Bold | 32pt | RGB 102/0/51 |
| Subtitle | Nunito Bold | 28pt | RGB 204/0/102 |
| Body text | Nunito Regular | 16-18pt | Black |
| Footnotes | Nunito Regular | 12pt | Black or Gray |

### Documents (Word)

| Element | Font | Size | Color |
|---------|------|------|-------|
| Headlines | Nunito Bold | 14-18pt | Black |
| Body text | Nunito Regular | 12pt | Black |
| Address/department | Nunito Regular | 10pt | Black |
| Footer info | Nunito Regular | 8pt | Black |

### Business Cards

| Element | Font | Size |
|---------|------|------|
| Name | Nunito Bold | 11pt |
| Title/position | Nunito Regular | 8pt |
| Contact info | Nunito Regular | 7pt |
| Website | Nunito Bold | 7pt |

### Email Signature

| Element | Font | Size | Color |
|---------|------|------|-------|
| Name | Nunito Bold | 10pt | Black or RGB 57/47/102 |
| Job position | Nunito Regular | 8pt | Black or RGB 57/47/102 |
| Contact details | Nunito Regular | 8pt | Black |

Note: For email signatures, Arial is acceptable as fallback since email clients may not render custom fonts.

## Font Characters

### Nunito
```
ABCDEFGHIJKLMNOPQRSTUVWXYZ
abcdefghijklmnopqrstuvwxyz
1234567890
|@#$%^&*(!?)-+/
```

## Line Spacing

- **Body text**: 1.15 - 1.5 line spacing
- **Headlines**: Single spacing
- **Paragraphs**: Add space after (6-12pt)

## Alignment

- **Headlines**: Left-aligned (default) or centered (for impact)
- **Body text**: Left-aligned, justified for formal documents
- **Contact info**: Right-aligned (letterhead), left-aligned (business cards)

## CSS Font Stack

```css
font-family: 'Nunito', Arial, sans-serif;
```

## Tagline Typography

"making it happen" / "Making IT happen"
- Font: Nunito Bold
- Case: Lowercase preferred for visual identity
- Color: White on colored backgrounds, Magenta (#EA2775) on white backgrounds
