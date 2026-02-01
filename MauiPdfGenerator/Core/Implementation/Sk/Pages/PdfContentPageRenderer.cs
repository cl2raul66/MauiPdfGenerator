using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Utils;
using MauiPdfGenerator.Core.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfContentPageRenderer : IPageRenderer
{
    public async Task<List<IReadOnlyList<PdfLayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        context.Logger.LogDebug("Starting layout orchestration for page definition. Root layout is {LayoutType}.", context.PageData.Content.GetType().Name);

        var pageBlocks = new List<IReadOnlyList<PdfLayoutInfo>>();
        var pageSize = SkiaUtils.GetSkPageSize(context.PageData.Size, context.PageData.Orientation);
        var pageMargins = context.PageData.Padding;

        var contentRect = new PdfRect(
            (float)pageMargins.Left,
            (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.HorizontalThickness,
            pageSize.Height - (float)pageMargins.VerticalThickness
        );

        PdfLayoutElementData? layoutToProcess = context.PageData.Content;

        while (layoutToProcess is not null)
        {
            var renderer = context.RendererFactory.GetRenderer(layoutToProcess);
            var elementContext = context with { Element = layoutToProcess };

            await renderer.MeasureAsync(elementContext, new SKSize(contentRect.Width, contentRect.Height));

            var arrangeInfo = await renderer.ArrangeAsync(contentRect, elementContext);

            if (arrangeInfo.Height <= 0 && arrangeInfo.RemainingElement is not null)
            {
                var culprit = arrangeInfo.RemainingElement;
                var culpritRenderer = context.RendererFactory.GetRenderer(culprit);
                var culpritContext = context with { Element = culprit };

                var measure = await culpritRenderer.MeasureAsync(culpritContext, new SKSize(contentRect.Width, float.PositiveInfinity));

                if (measure.Height > contentRect.Height)
                {
                    context.DiagnosticSink.Submit(new Diagnostics.Models.DiagnosticMessage(
                        Diagnostics.Enums.DiagnosticSeverity.Warning,
                        Diagnostics.DiagnosticCodes.PageContentOversized,
                        $"The element of type '{culprit.GetType().Name}' has a required height of {measure.Height}, which is larger than the available page height of {contentRect.Height}. The element will be skipped to prevent an infinite loop."
                    ));

                    layoutToProcess = null;
                    continue;
                }
            }

            if (arrangeInfo.FinalRect.HasValue && arrangeInfo.FinalRect.Value.Height > 0)
            {
                pageBlocks.Add([arrangeInfo]);
            }

            layoutToProcess = arrangeInfo.RemainingElement as PdfLayoutElementData;

            if (layoutToProcess is not null)
            {
                context.Logger.LogDebug("Content overflow detected. A new virtual page will be generated for the remaining content.");
            }
        }

        return pageBlocks;
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, IReadOnlyList<PdfLayoutInfo> arrangedPageBlock, PdfGenerationContext context)
    {
        if (arrangedPageBlock.Count != 1)
        {
            if (arrangedPageBlock.Any())
            {
                context.Logger.LogWarning("Expected a single root layout in the page block, but found {Count}. Rendering may be incorrect.", arrangedPageBlock.Count);
            }
            return;
        }

        var layoutInfo = arrangedPageBlock[0];
        var element = (PdfElementData)layoutInfo.Element;
        var renderer = context.RendererFactory.GetRenderer(element);
        var elementContext = context with { Element = element };
        await renderer.RenderAsync(canvas, elementContext);
    }
}
