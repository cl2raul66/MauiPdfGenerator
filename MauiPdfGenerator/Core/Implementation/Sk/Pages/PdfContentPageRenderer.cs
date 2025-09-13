using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using SkiaSharp;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfContentPageRenderer : IPageRenderer
{
    private readonly ContentPageOrchestrator _orchestrator = new();

    public async Task<List<IReadOnlyList<PdfLayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        context.Logger.LogDebug("Delegating layout orchestration for PdfContentPage to ContentPageOrchestrator.");
        var pageBlocks = new List<IReadOnlyList<PdfLayoutInfo>>();
        var elementsToProcess = new Queue<PdfElementData>(context.PageData.Elements);

        var pageSize = SkiaUtils.GetSkPageSize(context.PageData.Size, context.PageData.Orientation);
        var pageMargins = context.PageData.Padding;
        var contentRect = new PdfRect(
            (float)pageMargins.Left,
            (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.HorizontalThickness,
            pageSize.Height - (float)pageMargins.VerticalThickness
        );

        while (elementsToProcess.Count > 0)
        {
            var (arrangedPageElements, remainingForNextPage) = await _orchestrator.ProcessPageAsync(elementsToProcess, contentRect, context);

            if (arrangedPageElements.Count > 0)
            {
                pageBlocks.Add(arrangedPageElements.AsReadOnly());
            }
            else if (elementsToProcess.Count > 0 && remainingForNextPage.Count == elementsToProcess.Count)
            {
                var skippedElement = remainingForNextPage.Dequeue();
                context.Logger.LogWarning("Element {ElementType} is too large to fit on a page and was skipped.", skippedElement.GetType().Name);
            }

            elementsToProcess = remainingForNextPage;
        }

        return pageBlocks;
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, IReadOnlyList<PdfLayoutInfo> arrangedPageBlock, PdfGenerationContext context)
    {
        foreach (var layoutInfo in arrangedPageBlock)
        {
            var element = (PdfElementData)layoutInfo.Element;
            var renderer = context.RendererFactory.GetRenderer(element);
            var elementContext = context with { Element = element };
            await renderer.RenderAsync(canvas, elementContext);
        }
    }
}
