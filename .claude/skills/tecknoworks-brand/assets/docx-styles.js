/**
 * Tecknoworks Document Styles Module
 * 
 * Provides consistent branding for all Tecknoworks Word documents.
 * 
 * Usage:
 *   const tk = require('./docx-styles.js');
 *   const doc = tk.createDocument({ title: 'My Document', sections: [...] });
 *   // or use individual helpers:
 *   tk.createHeading1('Section Title')
 *   tk.createBullet('Bullet point text')
 */

const { Document, Packer, Paragraph, TextRun, ImageRun,
        Header, Footer, AlignmentType, PageBreak, HeadingLevel,
        BorderStyle, WidthType, ShadingType, Table, TableRow, TableCell,
        convertInchesToTwip, LevelFormat } = require('docx');
const fs = require('fs');

// ============================================================================
// BRAND COLORS
// ============================================================================
const COLORS = {
  darkGray: '515455',      // Text, secondary elements (Pantone 445 C)
  deepPurple: '320063',    // Accent, backgrounds (Pantone 2755 C)
  plum: '7D1C4B',          // Accent (Pantone 683 C)
  magenta: 'EA2775',       // Primary brand color (Pantone 7424 C)
  coral: 'EA8170',         // Accent, gradients (Pantone 170 C)
  white: 'FFFFFF',
  black: '000000',
  lightGray: 'F5F5F5'
};

// ============================================================================
// TYPOGRAPHY
// ============================================================================
const FONTS = {
  primary: 'Nunito',
  fallback: 'Arial'
};

const SIZES = {
  title: 56,          // Cover page main title
  subtitle: 44,       // Cover page subtitle
  tagline: 32,        // Tagline/highlight text
  h1: 36,             // Heading 1 (18pt)
  h2: 28,             // Heading 2 (14pt)
  h3: 24,             // Heading 3 (12pt)
  body: 22,           // Body text (11pt)
  small: 18,          // Small text (9pt)
  footer: 16          // Footer text (8pt)
};

// ============================================================================
// SPACING (in twentieths of a point - twips)
// ============================================================================
const SPACING = {
  paragraphAfter: 160,
  bulletAfter: 80,
  sectionGap: 200,
  h1Before: 400,
  h1After: 200,
  h2Before: 300,
  h2After: 180,
  h3Before: 240,
  h3After: 120
};

// ============================================================================
// LIST NUMBERING CONFIGURATION
// ============================================================================

// Reference IDs for list types
const LIST_REFS = {
  bullet: 'tecknoworks-bullets',
  numbered: 'tecknoworks-numbered'
};

/**
 * Get numbering configuration for proper Word lists
 * Defines Tecknoworks-branded bullet and numbered list styles
 */
function getNumberingConfig() {
  return {
    config: [
      {
        reference: LIST_REFS.bullet,
        levels: [
          {
            level: 0,
            format: LevelFormat.BULLET,
            text: '•',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 720, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          },
          {
            level: 1,
            format: LevelFormat.BULLET,
            text: '○',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 1440, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          },
          {
            level: 2,
            format: LevelFormat.BULLET,
            text: '▪',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 2160, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          }
        ]
      },
      {
        reference: LIST_REFS.numbered,
        levels: [
          {
            level: 0,
            format: LevelFormat.DECIMAL,
            text: '%1.',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 720, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          },
          {
            level: 1,
            format: LevelFormat.LOWER_LETTER,
            text: '%2.',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 1440, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          },
          {
            level: 2,
            format: LevelFormat.LOWER_ROMAN,
            text: '%3.',
            alignment: AlignmentType.LEFT,
            style: {
              paragraph: {
                indent: { left: 2160, hanging: 360 }
              },
              run: {
                color: COLORS.magenta,
                font: FONTS.primary,
                size: SIZES.body
              }
            }
          }
        ]
      }
    ]
  };
}

// ============================================================================
// HELPER FUNCTIONS - HEADINGS
// ============================================================================

/**
 * Create a Heading 1 paragraph (Magenta, Bold, 18pt)
 */
function createHeading1(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_1,
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.h1, 
      font: FONTS.primary, 
      color: COLORS.magenta 
    })]
  });
}

/**
 * Create a Heading 2 paragraph (Deep Purple, Bold, 14pt)
 */
function createHeading2(text) {
  return new Paragraph({
    heading: HeadingLevel.HEADING_2,
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.h2, 
      font: FONTS.primary, 
      color: COLORS.deepPurple 
    })]
  });
}

/**
 * Create a Heading 3 paragraph (Dark Gray, Bold, 12pt)
 */
function createHeading3(text) {
  return new Paragraph({
    spacing: { before: SPACING.h3Before, after: SPACING.h3After },
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.h3, 
      font: FONTS.primary, 
      color: COLORS.darkGray 
    })]
  });
}

// ============================================================================
// HELPER FUNCTIONS - PARAGRAPHS
// ============================================================================

/**
 * Create a regular body paragraph
 */
function createPara(text) {
  return new Paragraph({
    spacing: { after: SPACING.paragraphAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a labeled paragraph (e.g., "Core Mission: ...")
 * Label is bold and purple, followed by regular text
 */
function createLabeledPara(label, text) {
  return new Paragraph({
    spacing: { after: SPACING.paragraphAfter },
    children: [
      new TextRun({ 
        text: label + ' ', 
        bold: true, 
        size: SIZES.body, 
        font: FONTS.primary, 
        color: COLORS.deepPurple 
      }),
      new TextRun({ 
        text, 
        size: SIZES.body, 
        font: FONTS.primary 
      })
    ]
  });
}

/**
 * Create a bold paragraph
 */
function createBoldPara(text) {
  return new Paragraph({
    spacing: { after: SPACING.paragraphAfter },
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create an italic paragraph
 */
function createItalicPara(text) {
  return new Paragraph({
    spacing: { after: SPACING.paragraphAfter },
    children: [new TextRun({ 
      text, 
      italics: true, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

// ============================================================================
// HELPER FUNCTIONS - LISTS (Proper Word Lists)
// ============================================================================

/**
 * Create a bullet point (level 0) - proper Word list
 * @param {string} text - Bullet text
 */
function createBullet(text) {
  return new Paragraph({
    numbering: {
      reference: LIST_REFS.bullet,
      level: 0
    },
    spacing: { after: SPACING.bulletAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a sub-bullet (level 1) - proper Word list
 * @param {string} text - Sub-bullet text
 */
function createSubBullet(text) {
  return new Paragraph({
    numbering: {
      reference: LIST_REFS.bullet,
      level: 1
    },
    spacing: { after: SPACING.bulletAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a third-level bullet (level 2) - proper Word list
 * @param {string} text - Bullet text
 */
function createSubSubBullet(text) {
  return new Paragraph({
    numbering: {
      reference: LIST_REFS.bullet,
      level: 2
    },
    spacing: { after: SPACING.bulletAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a numbered list item (level 0) - proper Word list with auto-numbering
 * @param {string} text - Item text
 */
function createNumberedItem(text) {
  return new Paragraph({
    numbering: {
      reference: LIST_REFS.numbered,
      level: 0
    },
    spacing: { after: SPACING.bulletAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a sub-numbered item (level 1, e.g., a., b., c.)
 * @param {string} text - Item text
 */
function createSubNumberedItem(text) {
  return new Paragraph({
    numbering: {
      reference: LIST_REFS.numbered,
      level: 1
    },
    spacing: { after: SPACING.bulletAfter },
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary 
    })]
  });
}

/**
 * Create a bullet list from an array of strings
 * @param {string[]} items - Array of bullet point texts
 * @returns {Paragraph[]} Array of bullet paragraphs
 */
function createBulletList(items) {
  return items.map(text => createBullet(text));
}

/**
 * Create a numbered list from an array of strings
 * @param {string[]} items - Array of item texts
 * @returns {Paragraph[]} Array of numbered paragraphs
 */
function createNumberedList(items) {
  return items.map(text => createNumberedItem(text));
}

// ============================================================================
// HELPER FUNCTIONS - UTILITY
// ============================================================================

/**
 * Create a spacer paragraph
 */
function spacer(before = 0, after = SPACING.sectionGap) {
  return new Paragraph({
    spacing: { before, after },
    children: []
  });
}

/**
 * Create a page break
 */
function pageBreak() {
  return new Paragraph({ children: [new PageBreak()] });
}

/**
 * Create a horizontal line (using border)
 */
function horizontalLine(color = COLORS.magenta) {
  return new Paragraph({
    border: {
      bottom: { style: BorderStyle.SINGLE, size: 8, color }
    },
    spacing: { after: 200 },
    children: []
  });
}

// ============================================================================
// HELPER FUNCTIONS - COVER PAGE ELEMENTS
// ============================================================================

/**
 * Create the main cover title (centered, magenta, bold, 28pt)
 */
function createCoverTitle(text) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.title, 
      font: FONTS.primary, 
      color: COLORS.magenta 
    })]
  });
}

/**
 * Create cover subtitle (centered, purple, 22pt)
 */
function createCoverSubtitle(text) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { before: 400, after: 400 },
    children: [new TextRun({ 
      text, 
      size: SIZES.subtitle, 
      font: FONTS.primary, 
      color: COLORS.deepPurple 
    })]
  });
}

/**
 * Create a bordered tagline box for cover page
 */
function createTaglineBox(mainText, subText) {
  const children = [
    new TextRun({ 
      text: mainText, 
      bold: true, 
      size: SIZES.tagline, 
      font: FONTS.primary, 
      color: COLORS.deepPurple 
    })
  ];
  
  if (subText) {
    children.push(new TextRun({ text: '\n', font: FONTS.primary }));
    children.push(new TextRun({ 
      text: subText, 
      italics: true, 
      size: SIZES.h3, 
      font: FONTS.primary, 
      color: COLORS.darkGray 
    }));
  }
  
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { before: 800, after: 800 },
    border: {
      top: { style: BorderStyle.SINGLE, size: 8, space: 20, color: COLORS.magenta },
      bottom: { style: BorderStyle.SINGLE, size: 8, space: 20, color: COLORS.magenta }
    },
    children
  });
}

/**
 * Create version/date info for cover page
 */
function createVersionInfo(text) {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    children: [new TextRun({ 
      text, 
      size: SIZES.body, 
      font: FONTS.primary, 
      color: COLORS.darkGray 
    })]
  });
}

/**
 * Create confidential notice
 */
function createConfidentialNotice(text = 'CONFIDENTIAL') {
  return new Paragraph({
    alignment: AlignmentType.CENTER,
    spacing: { before: 200 },
    children: [new TextRun({ 
      text, 
      bold: true, 
      size: SIZES.small, 
      font: FONTS.primary, 
      color: COLORS.magenta 
    })]
  });
}

// ============================================================================
// HELPER FUNCTIONS - TABLES
// ============================================================================

/**
 * Create a styled table with Tecknoworks branding
 * @param {Array} headers - Array of header strings
 * @param {Array} rows - Array of row arrays
 */
function createTable(headers, rows) {
  const borderStyle = { style: BorderStyle.SINGLE, size: 1, color: COLORS.darkGray };
  const borders = { top: borderStyle, bottom: borderStyle, left: borderStyle, right: borderStyle };
  
  // Calculate column width (evenly distributed)
  const colWidth = Math.floor(9360 / headers.length);
  
  // Create header row
  const headerRow = new TableRow({
    children: headers.map(text => new TableCell({
      borders,
      width: { size: colWidth, type: WidthType.DXA },
      shading: { fill: COLORS.deepPurple, type: ShadingType.CLEAR },
      margins: { top: 80, bottom: 80, left: 120, right: 120 },
      children: [new Paragraph({
        children: [new TextRun({ 
          text, 
          bold: true, 
          size: SIZES.body, 
          font: FONTS.primary, 
          color: COLORS.white 
        })]
      })]
    }))
  });
  
  // Create data rows
  const dataRows = rows.map((row, rowIndex) => new TableRow({
    children: row.map(text => new TableCell({
      borders,
      width: { size: colWidth, type: WidthType.DXA },
      shading: { fill: rowIndex % 2 === 0 ? COLORS.white : COLORS.lightGray, type: ShadingType.CLEAR },
      margins: { top: 80, bottom: 80, left: 120, right: 120 },
      children: [new Paragraph({
        children: [new TextRun({ 
          text: String(text), 
          size: SIZES.body, 
          font: FONTS.primary 
        })]
      })]
    }))
  }));
  
  return new Table({
    width: { size: 100, type: WidthType.PERCENTAGE },
    columnWidths: headers.map(() => colWidth),
    rows: [headerRow, ...dataRows]
  });
}

// ============================================================================
// HEADER & FOOTER
// ============================================================================

/**
 * Create standard Tecknoworks header with logo
 * @param {Buffer} logoBuffer - Logo image buffer (PNG)
 */
function createHeader(logoBuffer) {
  return new Header({
    children: [
      new Paragraph({
        alignment: AlignmentType.RIGHT,
        children: [
          new ImageRun({
            type: 'png',
            data: logoBuffer,
            transformation: { width: 150, height: 40 },
            altText: { 
              title: 'Tecknoworks Logo', 
              description: 'Tecknoworks company logo', 
              name: 'logo' 
            }
          })
        ]
      })
    ]
  });
}

/**
 * Create standard Tecknoworks footer
 */
function createFooter() {
  return new Footer({
    children: [
      new Paragraph({
        alignment: AlignmentType.CENTER,
        children: [
          new TextRun({ 
            text: 'Tecknoworks | ', 
            size: SIZES.footer, 
            font: FONTS.primary, 
            color: COLORS.darkGray 
          }),
          new TextRun({ 
            text: 'www.tecknoworks.com', 
            size: SIZES.footer, 
            font: FONTS.primary, 
            color: COLORS.magenta 
          }),
          new TextRun({ 
            text: ' | Making IT Happen', 
            size: SIZES.footer, 
            font: FONTS.primary, 
            color: COLORS.darkGray 
          })
        ]
      })
    ]
  });
}

// ============================================================================
// DOCUMENT CREATION
// ============================================================================

/**
 * Get default document styles
 */
function getDocumentStyles() {
  return {
    default: {
      document: {
        run: { font: FONTS.primary, size: SIZES.body }
      }
    },
    paragraphStyles: [
      { 
        id: 'Heading1', 
        name: 'Heading 1', 
        basedOn: 'Normal', 
        next: 'Normal', 
        quickFormat: true,
        run: { size: SIZES.h1, bold: true, font: FONTS.primary, color: COLORS.magenta },
        paragraph: { spacing: { before: SPACING.h1Before, after: SPACING.h1After }, outlineLevel: 0 } 
      },
      { 
        id: 'Heading2', 
        name: 'Heading 2', 
        basedOn: 'Normal', 
        next: 'Normal', 
        quickFormat: true,
        run: { size: SIZES.h2, bold: true, font: FONTS.primary, color: COLORS.deepPurple },
        paragraph: { spacing: { before: SPACING.h2Before, after: SPACING.h2After }, outlineLevel: 1 } 
      },
      { 
        id: 'Heading3', 
        name: 'Heading 3', 
        basedOn: 'Normal', 
        next: 'Normal', 
        quickFormat: true,
        run: { size: SIZES.h3, bold: true, font: FONTS.primary, color: COLORS.darkGray },
        paragraph: { spacing: { before: SPACING.h3Before, after: SPACING.h3After }, outlineLevel: 2 } 
      },
    ]
  };
}

/**
 * Get default page properties
 */
function getPageProperties() {
  return {
    page: {
      size: { width: 12240, height: 15840 }, // US Letter
      margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } // 1 inch margins
    }
  };
}

/**
 * Create a complete Tecknoworks branded document
 * @param {Object} options - Document options
 * @param {Buffer} options.logoBuffer - Logo image buffer (required)
 * @param {Array} options.children - Document content paragraphs
 */
function createDocument(options) {
  const { logoBuffer, children } = options;
  
  if (!logoBuffer) {
    throw new Error('logoBuffer is required. Load with: fs.readFileSync(logoPath)');
  }
  
  return new Document({
    styles: getDocumentStyles(),
    numbering: getNumberingConfig(),
    sections: [{
      properties: getPageProperties(),
      headers: { default: createHeader(logoBuffer) },
      footers: { default: createFooter() },
      children: children || []
    }]
  });
}

/**
 * Save document to file
 * @param {Document} doc - docx Document instance
 * @param {string} filepath - Output file path
 */
async function saveDocument(doc, filepath) {
  const buffer = await Packer.toBuffer(doc);
  fs.writeFileSync(filepath, buffer);
  return filepath;
}

// ============================================================================
// EXPORTS
// ============================================================================

module.exports = {
  // Constants
  COLORS,
  FONTS,
  SIZES,
  SPACING,
  LIST_REFS,
  
  // Headings
  createHeading1,
  createHeading2,
  createHeading3,
  
  // Paragraphs
  createPara,
  createLabeledPara,
  createBoldPara,
  createItalicPara,
  
  // Lists (proper Word lists)
  createBullet,
  createSubBullet,
  createSubSubBullet,
  createNumberedItem,
  createSubNumberedItem,
  createBulletList,
  createNumberedList,
  
  // Utility
  spacer,
  pageBreak,
  horizontalLine,
  
  // Cover page
  createCoverTitle,
  createCoverSubtitle,
  createTaglineBox,
  createVersionInfo,
  createConfidentialNotice,
  
  // Tables
  createTable,
  
  // Header/Footer
  createHeader,
  createFooter,
  
  // Document config
  getDocumentStyles,
  getPageProperties,
  getNumberingConfig,
  
  // Document
  createDocument,
  saveDocument
};
