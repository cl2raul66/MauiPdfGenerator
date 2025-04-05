namespace MauiPdfGenerator.Fluent.Enums;

/// <summary>
/// Specifies text decorations like underline or strikethrough.
/// Can be combined using bitwise OR.
/// </summary>
[Flags]
public enum PdfTextDecorations
{
    /// <summary>
    /// No text decorations applied.
    /// </summary>
    None = 0,
    /// <summary>
    /// Underline text decoration.
    /// </summary>
    Underline = 1 << 0,
    /// <summary>
    /// Strikethrough text decoration.
    /// </summary>
    Strikethrough = 1 << 1
}
