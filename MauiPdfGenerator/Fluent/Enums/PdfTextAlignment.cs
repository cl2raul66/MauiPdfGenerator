namespace MauiPdfGenerator.Fluent.Enums;

/// <summary>
/// Specifies the horizontal or vertical alignment of text.
/// </summary>
public enum PdfTextAlignment
{
    /// <summary>
    /// Aligns text to the start (Left for LTR languages, Right for RTL).
    /// Aligns text to the top vertically.
    /// </summary>
    Start,
    /// <summary>
    /// Aligns text to the center.
    /// </summary>
    Center,
    /// <summary>
    /// Aligns text to the end (Right for LTR languages, Left for RTL).
    /// Aligns text to the bottom vertically.
    /// </summary>
    End
}
