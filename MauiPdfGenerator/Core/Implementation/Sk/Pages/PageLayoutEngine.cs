using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Layouts;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PageLayoutEngine
{
    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        var pageBlocks = new List<IReadOnlyList<LayoutInfo>>();
        var elementsToProcess = new Queue<PdfElementData>(context.PageData.Elements);

        var pageSize = SkiaUtils.GetSkPageSize(context.PageData.Size, context.PageData.Orientation);
        var pageMargins = context.PageData.Padding;
        var contentRect = new PdfRect(
            (float)pageMargins.Left,
            (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.HorizontalThickness,
            pageSize.Height - (float)pageMargins.VerticalThickness
        );

        context.Logger.LogDebug("Starting layout process. Content rect: {ContentRect}", contentRect);

        while (elementsToProcess.Count > 0)
        {
            var (arrangedPageElements, remainingForNextPage) = await ProcessSinglePageAsync(elementsToProcess, contentRect, context);

            if (arrangedPageElements.Count > 0)
            {
                pageBlocks.Add(arrangedPageElements.AsReadOnly());
                context.Logger.LogDebug("Completed page {PageNumber} with {ElementCount} elements",
                    pageBlocks.Count, arrangedPageElements.Count);
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

    private async Task<(List<LayoutInfo> ArrangedElements, Queue<PdfElementData> RemainingElements)> ProcessSinglePageAsync(
        Queue<PdfElementData> elements,
        PdfRect contentRect,
        PdfGenerationContext context)
    {
        var arrangedElementsOnPage = new List<LayoutInfo>();
        var currentY = contentRect.Top;
        var remainingHeight = contentRect.Height;

        while (elements.Count > 0)
        {
            var element = elements.Peek();
            var renderer = context.RendererFactory.GetRenderer(element);
            var elementContext = context with { Element = element };

            var availableRectForMeasure = new SKRect(0, 0, contentRect.Width, float.PositiveInfinity); // Medir siempre con espacio infinito
            var measureInfo = await renderer.MeasureAsync(elementContext, availableRectForMeasure);

            if (measureInfo.Height <= remainingHeight)
            {
                // El elemento cabe completo
                elements.Dequeue();
                var arrangeInfo = await ArrangeElementAsync(element, renderer, elementContext, contentRect, currentY, measureInfo.Height);
                arrangedElementsOnPage.Add(arrangeInfo);
                currentY += arrangeInfo.Height;
                remainingHeight -= arrangeInfo.Height;
            }
            else
            {
                // El elemento no cabe completo, ¿es divisible?
                if (IsElementDivisible(element))
                {
                    // Sí, es divisible. Le pedimos que se divida en el espacio restante.
                    elements.Dequeue();
                    var arrangeInfo = await ArrangeElementAsync(element, renderer, elementContext, contentRect, currentY, remainingHeight);

                    if (arrangeInfo.Height > 0)
                    {
                        arrangedElementsOnPage.Add(arrangeInfo);
                    }

                    if (arrangeInfo.RemainingElement != null)
                    {
                        var remainingQueue = elements.ToList();
                        remainingQueue.Insert(0, arrangeInfo.RemainingElement);
                        elements = new Queue<PdfElementData>(remainingQueue);
                    }
                }
                // Si no es divisible, o si es divisible pero no cabe ni un fragmento, la página está llena.
                break;
            }
        }

        return (arrangedElementsOnPage, elements);
    }

    private async Task<LayoutInfo> ArrangeElementAsync(
        PdfElementData element,
        IElementRenderer renderer,
        PdfGenerationContext elementContext,
        PdfRect contentRect,
        float currentY,
        float availableHeight)
    {
        var elementWidth = element.GetHorizontalOptions == LayoutAlignment.Fill
            ? contentRect.Width
            : (float)element.GetWidthRequest.GetValueOrDefault(contentRect.Width);

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

    private bool IsElementDivisible(PdfElementData element)
    {
        return element is PdfParagraphData;
    }
}
