namespace MauiPdfGenerator.Fluent.Primitives;

/// <summary>
/// Provides standard page sizes commonly used in PDF documents.
/// Sizes are represented using Microsoft.Maui.Graphics.Size.
/// The unit used depends on how these values are interpreted by the library (internally points).
/// </summary>
public static class StandardPageSize
{
    // Note: These values are directly in points (1/72 inch),
    // as this is the internal standard. The Fluent API methods accepting Size
    // might need to be aware of this or perform conversions if a specific unit
    // context is implied elsewhere. For direct use (like SetPageSize(StandardPageSize.A4)),
    // the library Core handles these point values correctly.

    /// <summary>ISO A4 (595.28 x 841.89 points).</summary>
    public static readonly Size A4 = new Size(595.276f, 841.89f);

    /// <summary>US Letter (612 x 792 points).</summary>
    public static readonly Size Letter = new Size(612f, 792f);

    /// <summary>US Legal (612 x 1008 points).</summary>
    public static readonly Size Legal = new Size(612f, 1008f);

    /// <summary>ISO A3 (841.89 x 1190.55 points).</summary>
    public static readonly Size A3 = new Size(841.89f, 1190.55f);

    /// <summary>ISO A5 (419.81 x 595.28 points).</summary>
    public static readonly Size A5 = new Size(419.811f, 595.276f);

    /// <summary>ISO B4 (708.66 x 1000.63 points).</summary>
    public static readonly Size B4_ISO = new Size(708.661f, 1000.63f);

    /// <summary>ISO B5 (498.90 x 708.66 points).</summary>
    public static readonly Size B5_ISO = new Size(498.898f, 708.661f);

    /// <summary>US Tabloid / Ledger (792 x 1224 points).</summary>
    public static readonly Size Tabloid = new Size(792f, 1224f);
}
