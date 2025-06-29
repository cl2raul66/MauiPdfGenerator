using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;
using System.Diagnostics;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class RenderPages
{
    private readonly ElementsRender _elementsRender = new();

    public async Task<Dictionary<int, IReadOnlyList<PdfElement>>> MeasureAsync(PdfPageData pageDef, PdfFontRegistryBuilder fontRegistry, Dictionary<PdfElement, object> layoutState)
    {
        var pageBlocks = new Dictionary<int, IReadOnlyList<PdfElement>>();
        var elements = new Queue<PdfElement>(pageDef.Elements);
        int pageIndex = 0;

        SKSize pageSize = SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        var pageMargins = pageDef.Margins;
        var contentRect = new SKRect(
            (float)pageMargins.Left, (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right, pageSize.Height - (float)pageMargins.Bottom
        );

        while (elements.Any())
        {
            var currentPageElements = new List<PdfElement>();
            float currentY = contentRect.Top;

            while (elements.Any())
            {
                var element = elements.Peek();
                var availableRectForMeasure = new SKRect(contentRect.Left, currentY, contentRect.Right, contentRect.Bottom);
                var measure = await _elementsRender.Measure(element, pageDef, availableRectForMeasure, currentY, layoutState, fontRegistry);

                if (measure.Error is not null)
                {
                    Debug.WriteLine($"[PAGINATION] Error measuring element {element.GetType().Name}: {measure.Error.Message}. Skipping.");
                    elements.Dequeue();
                    continue;
                }

                float elementHeight = measure.HeightRequired;
                bool needsNewPage = (currentY > contentRect.Top && elementHeight > availableRectForMeasure.Height) || measure.RequiresNewPage;

                if (needsNewPage)
                {
                    break;
                }

                elements.Dequeue();
                currentPageElements.Add(element);
                currentY += elementHeight;
                if (elements.Any())
                {
                    currentY += pageDef.PageDefaultSpacing;
                }

                if (measure.RemainingElement is not null)
                {
                    var tempList = new List<PdfElement> { measure.RemainingElement };
                    tempList.AddRange(elements);
                    elements = new Queue<PdfElement>(tempList);
                }
            }

            if (currentPageElements.Any())
            {
                pageBlocks.Add(pageIndex++, currentPageElements.AsReadOnly());
            }
            else if (elements.Any())
            {
                var element = elements.Dequeue();
                pageBlocks.Add(pageIndex++, new List<PdfElement> { element }.AsReadOnly());
            }
        }

        return pageBlocks;
    }

    public async Task RenderAsync(SKCanvas canvas, PdfPageData pageDef, IReadOnlyList<PdfElement> pageElements, PdfFontRegistryBuilder fontRegistry, Dictionary<PdfElement, object> layoutState)
    {
        canvas.Clear(pageDef.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageDef.BackgroundColor) : SKColors.White);

        SKSize pageSize = SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        var pageMargins = pageDef.Margins;
        var contentRect = new SKRect(
            (float)pageMargins.Left, (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right, pageSize.Height - (float)pageMargins.Bottom
        );

        float currentY = contentRect.Top;
        foreach (var element in pageElements)
        {
            var measure = await _elementsRender.Measure(element, pageDef, contentRect, currentY, layoutState, fontRegistry);
            var elementHeight = measure.HeightRequired;
            var elementWidth = measure.WidthRequired;

            float offsetX = element.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentRect.Width - elementWidth) / 2f,
                LayoutAlignment.End => contentRect.Width - elementWidth,
                _ => 0f
            };

            var elementRect = SKRect.Create(contentRect.Left + offsetX, currentY, elementWidth, elementHeight);
            await _elementsRender.Render(canvas, element, pageDef, elementRect, currentY, layoutState, fontRegistry);

            currentY += elementHeight;
            if (element != pageElements.Last())
            {
                currentY += pageDef.PageDefaultSpacing;
            }
        }
    }
}
