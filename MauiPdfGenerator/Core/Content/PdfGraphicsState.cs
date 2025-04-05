using MauiPdfGenerator.Core.Fonts; // Assuming PdfFontBase exists
using System.Numerics; // For Matrix3x2

namespace MauiPdfGenerator.Core.Content;

/// <summary>
/// Helper class to track the current graphics state within a content stream.
/// Used by PdfContentStream to optimize output by avoiding redundant state changes.
/// (Initial simple version)
/// </summary>
internal class PdfGraphicsState
{
    // Tracked properties (add more as needed: LineWidth, LineCap, LineJoin, MiterLimit, RenderingIntent, etc.)
    public Matrix3x2 CurrentTransform { get; set; } = Matrix3x2.Identity;
    public PdfColorState? StrokeColor { get; set; } // Using a helper struct/class for color spaces
    public PdfColorState? FillColor { get; set; }
    public PdfFontBase? CurrentFont { get; set; }
    public double? CurrentFontSize { get; set; }
    public double CharacterSpacing { get; set; } = 0;
    public double WordSpacing { get; set; } = 0;
    public double HorizontalScaling { get; set; } = 100; // Percentage
    public double TextLeading { get; set; } = 0;

    // Text state specific
    public Matrix3x2 TextMatrix { get; set; } = Matrix3x2.Identity;
    public Matrix3x2 TextLineMatrix { get; set; } = Matrix3x2.Identity;


    // TODO: Implement methods to compare current state with desired state
    // bool IsFontChanged(PdfFontBase newFont, double newSize) => CurrentFont != newFont || CurrentFontSize != newSize;
    // bool IsFillColorChanged(...)
    // bool IsTransformChanged(...)

    // TODO: Need PdfColorState helper to handle different color spaces (Gray, RGB, CMYK)
}

/// <summary>
/// Placeholder for representing color state including color space.
/// </summary>
internal struct PdfColorState
{
    public object ColorSpaceNameOrArray { get; set; } // e.g., PdfName.DeviceRGB or an array for Indexed/ICC
    public double[] ColorComponents { get; set; } // e.g., {r, g, b} or {g} or {c, m, y, k}

    // TODO: Equality comparison, methods to generate PDF operators (rg, RG, k, K, cs, CS, sc, SC, scn, SCN)
}
