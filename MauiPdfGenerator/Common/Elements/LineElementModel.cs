using MauiPdfGenerator.Common.Primitives;

namespace MauiPdfGenerator.Common.Elements;

/// <summary>
/// Represents a line segment to be drawn on the page.
/// </summary>
internal class LineElementModel : ElementModel // Position here represents the StartPoint
{
    /// <summary>
    /// The end point of the line segment.
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfPoint EndPoint { get; set; }

    /// <summary>
    /// The thickness (stroke width) of the line. Defaults to 1.
    /// Unit is typically interpreted as points by the Core, regardless of DocumentSettings.Units.
    /// </summary>
    public float Thickness { get; set; } = 1f;

    /// <summary>
    /// The color of the line. Defaults to Black.
    /// </summary>
    public PdfColor Color { get; set; } = PdfColor.Black;

    /// <summary>
    /// The start point of the line segment (same as Position).
    /// Units are defined by DocumentSettings.Units.
    /// </summary>
    public PdfPoint StartPoint
    {
        get => Position;
        set => Position = value;
    }
}
