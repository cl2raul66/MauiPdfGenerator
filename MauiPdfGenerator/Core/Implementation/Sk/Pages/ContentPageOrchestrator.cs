using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Diagnostics;
using MauiPdfGenerator.Diagnostics.Enums;
using MauiPdfGenerator.Diagnostics.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class ContentPageOrchestrator
{
    public async Task<(List<PdfLayoutInfo> ArrangedElements, Queue<PdfElementData> RemainingElements)> ProcessPageAsync(
        Queue<PdfElementData> elements,
        PdfRect contentRect,
        PdfGenerationContext context)
    {
        var arrangedElementsOnPage = new List<PdfLayoutInfo>();
        var spilloverElements = new List<PdfElementData>();
        var currentY = contentRect.Top;
        var remainingHeight = contentRect.Height;

        context.Logger.LogTrace("Starting layout for new page. Available height: {RemainingHeight}", remainingHeight);

        while (elements.Count > 0)
        {
            var element = elements.Peek();
            var renderer = context.RendererFactory.GetRenderer(element);
            var elementContext = context with { Element = element };

            context.Logger.LogTrace("Measuring element {ElementType}", element.GetType().Name);
            var availableRectForMeasure = new SKRect(0, 0, contentRect.Width, float.PositiveInfinity);
            var measureInfo = await renderer.MeasureAsync(elementContext, availableRectForMeasure);
            context.Logger.LogTrace("Element measured. Desired height: {DesiredHeight}", measureInfo.Height);

            if (measureInfo.Height <= remainingHeight)
            {
                // El elemento cabe completo en el espacio restante.
                elements.Dequeue();
                var arrangeInfo = await ArrangeElementAsync(element, renderer, elementContext, contentRect, currentY, measureInfo.Height);
                arrangedElementsOnPage.Add(arrangeInfo);
                currentY += arrangeInfo.Height;
                remainingHeight -= arrangeInfo.Height;
                context.Logger.LogTrace("Element arranged. Consumed height: {ConsumedHeight}, Remaining height: {RemainingHeight}", arrangeInfo.Height, remainingHeight);
            }
            else
            {
                // El elemento no cabe completo.
                elements.Dequeue(); // Lo consumimos de la cola actual.

                // Le pedimos que se disponga en el espacio que SÍ queda.
                var arrangeInfo = await ArrangeElementAsync(element, renderer, elementContext, contentRect, currentY, remainingHeight);

                if (arrangeInfo.Height > 0)
                {
                    // Una parte del elemento cupo (caso de elemento divisible).
                    arrangedElementsOnPage.Add(arrangeInfo);
                }

                if (arrangeInfo.RemainingElement is not null)
                {
                    // El resto va a la siguiente página.
                    spilloverElements.Add(arrangeInfo.RemainingElement);
                }
                else if (arrangeInfo.Height <= 0)
                {
                    // No cupo nada del elemento (era atómico o no cabía ni un fragmento).
                    if (measureInfo.Height > contentRect.Height)
                    {
                        // El elemento es más grande que una página completa. Se omite.
                        context.DiagnosticSink.Submit(new DiagnosticMessage(
                            DiagnosticSeverity.Warning,
                            DiagnosticCodes.PageContentOversized,
                            $"The element of type '{element.GetType().Name}' has a required height of {measureInfo.Height}, which is larger than the available page height of {contentRect.Height}. The element will be skipped.",
                            new DiagnosticRect(contentRect.X, currentY, contentRect.Width, measureInfo.Height)
                        ));
                    }
                    else
                    {
                        // Lo devolvemos a la cola para la siguiente página.
                        spilloverElements.Add(element);
                    }
                }

                // La página está llena, ya sea porque un elemento se dividió o porque uno atómico no cupo.
                break;
            }
        }

        // Preparamos la cola para la siguiente página, añadiendo primero los elementos sobrantes.
        var nextQueue = new Queue<PdfElementData>();
        foreach (var item in spilloverElements)
        {
            nextQueue.Enqueue(item);
        }
        foreach (var item in elements)
        {
            nextQueue.Enqueue(item);
        }

        context.Logger.LogTrace("Page composition complete. {ElementCount} elements placed.", arrangedElementsOnPage.Count);
        return (arrangedElementsOnPage, nextQueue);
    }

    private async Task<PdfLayoutInfo> ArrangeElementAsync(
        PdfElementData element,
        IElementRenderer renderer,
        PdfGenerationContext elementContext,
        PdfRect contentRect,
        float currentY,
        float availableHeight)
    {
        // Medimos de nuevo solo para obtener el ancho deseado, ya que la altura la define availableHeight.
        var measureInfo = await renderer.MeasureAsync(elementContext, new SKRect(0, 0, contentRect.Width, float.PositiveInfinity));

        var elementWidth = element.GetHorizontalOptions is LayoutAlignment.Fill
            ? contentRect.Width
            : Math.Min(measureInfo.Width, contentRect.Width);

        var offsetX = element.GetHorizontalOptions switch
        {
            LayoutAlignment.Center => (contentRect.Width - elementWidth) / 2f,
            LayoutAlignment.End => contentRect.Width - elementWidth,
            _ => 0f
        };

        var finalRect = new PdfRect(
            contentRect.Left + offsetX,
            currentY,
            elementWidth,
            availableHeight);

        var arrangeInfo = await renderer.ArrangeAsync(finalRect, elementContext);
        return arrangeInfo;
    }
}
