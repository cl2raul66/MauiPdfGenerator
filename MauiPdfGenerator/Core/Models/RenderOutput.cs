using MauiPdfGenerator.Fluent.Models.Elements;

namespace MauiPdfGenerator.Core.Models;

internal readonly record struct RenderOutput
{
    public float HeightDrawnThisCall { get; }
    public PdfElement? RemainingElement { get; }
    public bool RequiresNewPage { get; }
    public RenderOutput(float heightDrawnThisCall, PdfElement? remainingElement, bool requiresNewPage)
    {
        HeightDrawnThisCall = heightDrawnThisCall;
        RemainingElement = remainingElement;
        RequiresNewPage = requiresNewPage;
    }
}
