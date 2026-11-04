---
name: tecknoworks-brand
description: "Tecknoworks corporate brand guidelines for creating branded presentations and documents. Use when: (1) Creating PowerPoint presentations with Tecknoworks branding, (2) Creating Word documents with Tecknoworks letterhead/stationery, (3) Programmatically generating DOCX files with brand styling, (4) Any design work requiring Tecknoworks colors, typography, or logo usage, (5) User mentions Tecknoworks brand, visual identity, or corporate materials"
---

# Tecknoworks Brand Guidelines Skill

Apply Tecknoworks brand identity consistently across presentations, documents, and other materials.

## Brand Overview

**Tagline**: "Making it happen" / "Making IT happen"  
**Core attributes**: Technology, Knowledge, Team Spirit  
**Brand personality**: Technical yet approachable, passionate, visionary

## Quick Reference

### Color Palette

| Color Name | Pantone | HEX | RGB | Use |
|------------|---------|-----|-----|-----|
| Dark Gray | 445 C | #515455 | 77, 80, 88 | Text, secondary elements |
| Deep Purple | 2755 C | #320063 | 54, 47, 102 | Accent, backgrounds |
| Plum | 683 C | #7D1C4B | 149, 50, 99 | Accent |
| Magenta | 7424 C | #EA2775 | 225, 48, 104 | Primary brand color |
| Coral | 170 C | #EA8170 | 237, 129, 111 | Accent, gradients |

### Typography

| Use | Font | Weight | Fallback |
|-----|------|--------|----------|
| Headlines | Nunito | Bold (700) | Arial Bold |
| Body text | Nunito | Regular (400) | Arial |
| Footnotes | Nunito | Regular (400) | Arial |

**Font source**: [Google Fonts - Nunito](https://fonts.google.com/specimen/Nunito)

**Font embedding for documents**: When creating Word documents, enable font embedding (File → Options → Save → "Embed fonts in the file") to ensure recipients see correct fonts. For distribution, export to PDF.

---

## Creating Branded Word Documents (DOCX)

### Using the Style Module

The `docx-styles.js` module provides consistent Tecknoworks branding for all Word documents.

#### Setup

```bash
npm install docx
```

```javascript
const tk = require('./assets/docx-styles.js');
const fs = require('fs');

// Load the logo (required for header)
const logoBuffer = fs.readFileSync('./assets/logo-color-dark.png');
```

#### Quick Start Example

```javascript
const doc = tk.createDocument({
  logoBuffer,
  children: [
    // Cover page
    tk.spacer(2000, 0),
    tk.createCoverTitle('TECKNOWORKS'),
    tk.createCoverSubtitle('Report Title'),
    tk.createTaglineBox('Main Message', 'Supporting text'),
    tk.createVersionInfo('Version 1.0  |  January 2026'),
    tk.createConfidentialNotice(),
    
    tk.pageBreak(),
    
    // Content
    tk.createHeading1('Section Title'),
    tk.createPara('Regular paragraph text goes here.'),
    tk.createBullet('First bullet point'),
    tk.createBullet('Second bullet point'),
    tk.createSubBullet('Nested bullet'),
    
    tk.createHeading2('Subsection'),
    tk.createLabeledPara('Key Point:', 'Important information here.'),
    
    // Auto-numbered list (no manual numbers needed)
    tk.createNumberedItem('First numbered item'),
    tk.createNumberedItem('Second numbered item'),
    tk.createSubNumberedItem('Sub-item a'),
    tk.createSubNumberedItem('Sub-item b'),
    
    // Or use convenience functions for lists
    ...tk.createBulletList(['Item one', 'Item two', 'Item three']),
    ...tk.createNumberedList(['Step one', 'Step two', 'Step three']),
  ]
});

// Save the document
await tk.saveDocument(doc, 'output.docx');
```

### Available Functions

#### Headings
| Function | Description | Style |
|----------|-------------|-------|
| `createHeading1(text)` | Main section heading | Magenta, Bold, 18pt |
| `createHeading2(text)` | Subsection heading | Deep Purple, Bold, 14pt |
| `createHeading3(text)` | Minor heading | Dark Gray, Bold, 12pt |

#### Paragraphs
| Function | Description |
|----------|-------------|
| `createPara(text)` | Regular body paragraph |
| `createLabeledPara(label, text)` | Bold purple label + regular text |
| `createBoldPara(text)` | Bold paragraph |
| `createItalicPara(text)` | Italic paragraph |

#### Lists (Proper Word Lists)
| Function | Description | Style |
|----------|-------------|-------|
| `createBullet(text)` | Bullet point (level 0) | Magenta • |
| `createSubBullet(text)` | Second-level bullet (level 1) | Magenta ○ |
| `createSubSubBullet(text)` | Third-level bullet (level 2) | Magenta ▪ |
| `createNumberedItem(text)` | Auto-numbered item (level 0) | Magenta 1. 2. 3. |
| `createSubNumberedItem(text)` | Sub-numbered item (level 1) | Magenta a. b. c. |
| `createBulletList(items)` | Bullet list from array | Returns Paragraph[] |
| `createNumberedList(items)` | Numbered list from array | Returns Paragraph[] |

Lists are rendered as proper Word lists with automatic numbering and correct list behavior (Tab to indent, restart numbering, etc.).

#### Cover Page Elements
| Function | Description |
|----------|-------------|
| `createCoverTitle(text)` | Main title (56pt, Magenta) |
| `createCoverSubtitle(text)` | Subtitle (44pt, Purple) |
| `createTaglineBox(main, sub)` | Bordered tagline box |
| `createVersionInfo(text)` | Version/date line |
| `createConfidentialNotice(text)` | Confidentiality notice |

#### Tables
```javascript
tk.createTable(
  ['Header 1', 'Header 2', 'Header 3'],  // Headers
  [
    ['Row 1 Col 1', 'Row 1 Col 2', 'Row 1 Col 3'],
    ['Row 2 Col 1', 'Row 2 Col 2', 'Row 2 Col 3'],
  ]
);
```

#### Utility
| Function | Description |
|----------|-------------|
| `spacer(before, after)` | Vertical spacing |
| `pageBreak()` | Page break |
| `horizontalLine(color)` | Horizontal rule |

### Style Constants

Access brand constants directly:

```javascript
tk.COLORS.magenta     // 'EA2775'
tk.COLORS.deepPurple  // '320063'
tk.COLORS.darkGray    // '515455'

tk.FONTS.primary      // 'Nunito'
tk.FONTS.fallback     // 'Arial'

tk.SIZES.h1           // 36 (half-points)
tk.SIZES.body         // 22

tk.LIST_REFS.bullet   // 'tecknoworks-bullets'
tk.LIST_REFS.numbered // 'tecknoworks-numbered'
```

### Document Specifications

- **Page size**: US Letter (8.5" x 11")
- **Margins**: 1 inch all sides
- **Default font**: Nunito 11pt
- **Header**: Logo right-aligned
- **Footer**: "Tecknoworks | www.tecknoworks.com | Making IT Happen" (centered)

---

## Creating Branded Presentations

### PowerPoint Specifications

- **Screen formats**: 16:9 (preferred), 4:3
- **Title**: RGB 234/39/117 (#EA2775), 32pt Nunito Bold
- **Subtitle**: RGB 50/0/99 (#320063), 28pt Nunito Bold
- **Body text**: Black, 16pt Nunito Regular
- **Logo**: ~1/3 screen width, upper right corner preferred
- **Background**: White preferred for on-brand consistency

### Creating Branded Presentations

When creating PowerPoint presentations:

1. Use white background with Tecknoworks logo in upper right
2. Apply brand colors for headers (magenta/purple tones)
3. Use Nunito Bold for titles, Nunito Regular for body (fallback: Arial)
4. Include geometric triangle pattern from brand assets on accent slides
5. Place "making it happen" tagline on closing slides

### Slide Layout Zones

```
┌────────────────────────────────────────┐
│ [Title area - brand colors]      [Logo]│
├────────────────────────────────────────┤
│                                        │
│     [Content area - black text]        │
│                                        │
├────────────────────────────────────────┤
│ [Footer - web address/tagline]         │
└────────────────────────────────────────┘
```

---

## Creating Branded Documents

When creating Word documents manually:

1. Logo positioned top right with 16mm clear space
2. All body text in black, Nunito 12pt (fallback: Arial)
3. Company information in footer, Nunito 8pt
4. Include geometric accent pattern on cover pages only
5. **Enable font embedding** or export to PDF for distribution

### Letterhead Structure

```
┌────────────────────────────────────────┐
│                           [Logo 16mm→] │
│                                        │
│ [Content area - 8mm margins]           │
│                                        │
│                                        │
├────────────────────────────────────────┤
│ [geometric] [Company info - 8pt]       │
└────────────────────────────────────────┘
```

### Document Specifications

- **Letterhead logo**: Top right, 16mm from edge
- **Footer text**: Nunito 8pt
- **Body text**: Nunito 12pt
- **Address/department**: Nunito 10pt
- **Text color**: Black only on letterhead
- **Clear space**: 8mm from logo to text
- **Font embedding**: Enable in Word for proper rendering

---

## Brand Assets

| Asset | Path | Use |
|-------|------|-----|
| Logo (color, dark text) | `assets/logo-color-dark.svg` | Light backgrounds (preferred) |
| Logo (color, dark PNG) | `assets/logo-color-dark.png` | For DOCX embedding |
| Logo (color, white text) | `assets/logo-color-white.svg` | Dark backgrounds |
| Logo (monochrome) | `assets/logo-mono.svg` | Single-color printing |
| Logo + tagline (color) | `assets/logo-tagline-color-dark.svg` | Light backgrounds with tagline |
| Logo + tagline (white) | `assets/logo-tagline-white.svg` | Dark backgrounds with tagline |
| Logo + tagline (mono) | `assets/logo-tagline-mono.svg` | Single-color with tagline |
| **DOCX Style Module** | `assets/docx-styles.js` | Programmatic document generation |
| **Template Document** | `assets/template.docx` | Reference/starting point |

All logos are SVG (vector format, infinitely scalable) unless noted.

---

## Logo Usage Rules

**DO:**
- Use full-color logo on white backgrounds (preferred)
- Use white reverse logo on dark backgrounds (50%+ black)
- Maintain clear space equal to half the logo height

**DON'T:**
- Delete any portion of the logo
- Use non-approved colors
- Place elements in clear space
- Use as repeated pattern
- Use on backgrounds with insufficient contrast
- Combine with other logos or text

---

## Brand Values

Apply these principles when creating content:

1. **Honesty & Integrity** - Transparent, reliable
2. **Do it right** - Quality always wins
3. **Do it right first time** - Lean, focused on quality
4. **Thought-partnership** - Two-way dialogue with customers
5. **People first** - Invest in talent
6. **Inclusive for all** - Equal opportunity

---

## Detailed Guidelines

For comprehensive specifications, see:

- [`references/colors.md`](references/colors.md) - Complete color system with tints and usage
- [`references/logo.md`](references/logo.md) - Logo usage, clear space, and restrictions
- [`references/typography.md`](references/typography.md) - Font specifications and hierarchy
- [`references/templates.md`](references/templates.md) - Document and presentation templates
