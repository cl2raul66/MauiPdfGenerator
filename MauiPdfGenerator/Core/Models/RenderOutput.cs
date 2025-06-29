using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Models;

internal interface IPdfRenderService
{
    void RenderDocument(PdfDocumentData document, Stream output);
    void RenderPage(PdfPageData page, Stream output);
}

internal interface IPdfElementRenderer<TElement> where TElement : PdfElement
{
    void Render(TElement element, RenderContext context);
}

internal class RenderContext
{
    public object Target { get; }
    public System.Drawing.RectangleF Bounds { get; }

    public RenderContext(object target, System.Drawing.RectangleF bounds)
    {
        Target = target;
        Bounds = bounds;
    }
}

internal interface IRenderOutput
{
    float Width { get; }
    float Height { get; }
}

internal readonly record struct RenderOutput
{
    public float HeightDrawnThisCall { get; }

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
