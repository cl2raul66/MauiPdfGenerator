using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common.Elements;

/// <summary>
/// Represents a rectangle shape to be drawn on the page.
/// </summary>
internal class RectangleElementModel : ElementModel // Position here represents the Top-Left corner
{
    /// <summary>
    /// The size (width and height) of the rectangle.
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfSize Size { get; set; }

    /// <summary>
    /// The thickness of the rectangle's border (stroke).
    /// A value of 0 or less means no border is drawn. Defaults to 1.
    /// Unit is typically interpreted as points by the Core.
    /// </summary>
    public float StrokeThickness { get; set; } = 1f;

    /// <summary>
    /// The color of the rectangle's border (stroke). Defaults to Black.
    /// </summary>
    public PdfColor StrokeColor { get; set; } = PdfColor.Black;

    /// <summary>
    /// The color used to fill the rectangle's interior. Defaults to Transparent (no fill).
    /// </summary>
    public PdfColor FillColor { get; set; } = PdfColor.Transparent;

    /// <summary>
    /// Convenience property to get the full rectangle bounds based on Position and Size.
    /// </summary>
    public PdfRect Bounds => new PdfRect(Position, Size);
}
