using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
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

            var arrangeInfo = await renderer.ArrangeAsync(contentRect, elementContext);

            // --- INICIO DE LA LÓGICA ANTI-BUCLE DEFINITIVA ---
            // Condición de bucle infinito: No se pudo colocar nada en la página (altura cero o negativa),
            // pero el layout sigue reportando que tiene contenido sobrante.
            if (arrangeInfo.Height <= 0 && arrangeInfo.RemainingElement != null)
            {
                // Estamos atascados. El elemento sobrante es el culpable.
                var culprit = arrangeInfo.RemainingElement;
                var culpritRenderer = context.RendererFactory.GetRenderer(culprit);
                var culpritContext = context with { Element = culprit };

                // Medimos el elemento culpable para ver si es simplemente demasiado grande para cualquier página.
                var measure = await culpritRenderer.MeasureAsync(culpritContext, new SKRect(0, 0, contentRect.Width, float.PositiveInfinity));

                if (measure.Height > contentRect.Height)
                {
                    // El elemento es más grande que una página. Es un error irrecuperable.
                    context.DiagnosticSink.Submit(new Diagnostics.Models.DiagnosticMessage(
                        Diagnostics.Enums.DiagnosticSeverity.Warning,
                        Diagnostics.DiagnosticCodes.PageContentOversized,
                        $"The element of type '{culprit.GetType().Name}' has a required height of {measure.Height}, which is larger than the available page height of {contentRect.Height}. The element will be skipped to prevent an infinite loop."
                    ));

                    // Rompemos el bucle descartando todo el contenido restante.
                    layoutToProcess = null;
                    continue; // Salta a la siguiente iteración del while, que fallará y terminará.
                }
            }
            // --- FIN DE LA LÓGICA ANTI-BUCLE ---

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
            // Este log puede aparecer si un elemento se saltó, es normal en ese caso.
            if (arrangedPageBlock.Any())
            {
                context.Logger.LogWarning("Expected a single root layout in the page block, but found {Count}. Rendering may be incorrect.", arrangedPageBlock.Count);
            }
            return;
        }

        var layoutInfo = arrangedPageBlock[0];
        var element = (Common.Models.PdfElementData)layoutInfo.Element;
        var renderer = context.RendererFactory.GetRenderer(element);
        var elementContext = context with { Element = element };
        await renderer.RenderAsync(canvas, elementContext);
    }
}
