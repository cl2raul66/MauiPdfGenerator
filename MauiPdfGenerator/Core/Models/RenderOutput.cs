using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct RenderOutput
{
    /// <summary>
    /// The total height consumed by the element for vertical layout purposes.
    /// This is used to advance the Y-cursor for the next element.
    /// For text, this includes full line spacing.
    /// </summary>
    public float HeightDrawnThisCall { get; }

    /// <summary>
    /// The visual "inked" height of the element.
    /// This is used for calculating tight container backgrounds (e.g., HorizontalStackLayout).
    /// For text, this does not include the extra leading below the last line.
    /// For other elements, this is typically the same as HeightDrawnThisCall.
    /// </summary>
    public float VisualHeightDrawn { get; }

    public float WidthDrawnThisCall { get; }
    public PdfElement? RemainingElement { get; }
    public bool RequiresNewPage { get; }

    public RenderOutput(float heightDrawnThisCall, float widthDrawnThisCall, PdfElement? remainingElement, bool requiresNewPage, float? visualHeightDrawn = null)
    {
        HeightDrawnThisCall = heightDrawnThisCall;
        WidthDrawnThisCall = widthDrawnThisCall;
        RemainingElement = remainingElement;
        RequiresNewPage = requiresNewPage;
        VisualHeightDrawn = visualHeightDrawn ?? heightDrawnThisCall;
    }
}
