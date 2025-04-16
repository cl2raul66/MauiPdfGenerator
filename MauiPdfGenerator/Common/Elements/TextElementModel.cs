using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common.Elements;

/// <summary>
/// Represents a text element to be drawn on the page.
/// </summary>
internal class TextElementModel : ElementModel
{
    /// <summary>
    /// The text content to display.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The font to use for rendering the text.
    /// </summary>
    public PdfFont Font { get; set; }

    /// <summary>
    /// The color of the text. Defaults to Black.
    /// </summary>
    public PdfColor Color { get; set; } = PdfColor.Black;

    /// <summary>
    /// Horizontal alignment of the text relative to the Position.X coordinate.
    /// </summary>
    public PdfHorizontalAlignment HorizontalAlignment { get; set; } = PdfHorizontalAlignment.Left;

    /// <summary>
    /// Vertical alignment of the text relative to the Position.Y coordinate.
    /// (SkiaSharp often uses baseline, this might need careful handling in the renderer).
    /// </summary>
    public PdfVerticalAlignment VerticalAlignment { get; set; } = PdfVerticalAlignment.Top;

    /// <summary>
    /// Optional maximum width for basic text wrapping. Null or non-positive value means no wrapping.
    /// Unit depends on DocumentSettings.Units.
    /// </summary>
    public float? MaxWidth { get; set; } = null;
}
