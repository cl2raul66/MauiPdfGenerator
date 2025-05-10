using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Core.Models;

/// <summary>
/// Represents the result of a rendering operation for a PDF element.
/// </summary>
internal readonly record struct RenderOutput
{
    /// <summary>
    /// The actual height drawn on the current page for this call.
    /// </summary>
    public float HeightDrawnThisCall { get; }

    /// <summary>
    /// The remaining part of the element if it was split (e.g., a new PdfParagraph with remaining lines),
    /// or the original element if it needs to be moved entirely to a new page (e.g., a PdfImage).
    /// Null if the element was completely rendered or discarded (e.g., image too large).
    /// </summary>
    public PdfElement? RemainingElement { get; }

    /// <summary>
    /// True if the element (or its remainder) could not be rendered on the current page
    /// and requires a new page to be started.
    /// </summary>
    public bool RequiresNewPage { get; }

    public RenderOutput(float heightDrawnThisCall, PdfElement? remainingElement, bool requiresNewPage)
    {
        HeightDrawnThisCall = heightDrawnThisCall;
        RemainingElement = remainingElement;
        RequiresNewPage = requiresNewPage;
    }
}
