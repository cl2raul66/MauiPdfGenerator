using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common;

/// <summary>
/// Contains global settings and metadata for the PDF document.
/// </summary>
internal class DocumentSettings
{
    // --- Default Page Layout ---
    /// <summary>
    /// Default page size if not specified per page (e.g., A4, Letter).
    /// Units are defined by the Units property.
    /// </summary>
    public PdfSize DefaultPageSize { get; set; } = PageSizes.A4; // Use helper below

    /// <summary>
    /// Default page margins if not specified per page.
    /// Units are defined by the Units property.
    /// </summary>
    public PdfMargins DefaultMargins { get; set; } = new PdfMargins(25.4f); // Default: ~1 inch in mm

    /// <summary>
    /// The primary unit of measure used when defining sizes and positions
    /// via the public Fluent API. The Core will convert this to points.
    /// </summary>
    public UnitOfMeasure Units { get; set; } = UnitOfMeasure.Millimeters; // Default Unit

    /// <summary>
    /// Default font used if an element doesn't specify one.
    /// Size unit matches the Units property.
    /// </summary>
    public PdfFont DefaultFont { get; set; } = new PdfFont("Helvetica", 10, PdfFontStyle.Normal);

    // --- PDF Metadata ---
    public string Author { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty; // Comma or space separated
    public string Creator { get; set; } = "MauiPdfGenerator"; // Optional: Identify the generator

    // --- Standard Page Sizes Helper --- (Defined in Points)
    // Store sizes internally in points for easier Core conversion
    public static class PageSizes
    {
        /// <summary>ISO A4 (210 x 297 mm) in points.</summary>
        public static readonly PdfSize A4 = new PdfSize(595.276f, 841.89f);
        /// <summary>US Letter (8.5 x 11 inches) in points.</summary>
        public static readonly PdfSize Letter = new PdfSize(612f, 792f);
        /// <summary>US Legal (8.5 x 14 inches) in points.</summary>
        public static readonly PdfSize Legal = new PdfSize(612f, 1008f);
        /// <summary>ISO A3 (297 x 420 mm) in points.</summary>
        public static readonly PdfSize A3 = new PdfSize(841.89f, 1190.55f);
        /// <summary>ISO A5 (148 x 210 mm) in points.</summary>
        public static readonly PdfSize A5 = new PdfSize(419.811f, 595.276f);

        // Add more sizes as needed (B4, B5, Tabloid, etc.)
    }
}
