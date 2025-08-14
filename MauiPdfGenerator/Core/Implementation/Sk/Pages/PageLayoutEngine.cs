using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using Microsoft.Extensions.Logging;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PageLayoutEngine
{
    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        var pageBlocks = new List<IReadOnlyList<LayoutInfo>>();
        var elementsToProcess = new Queue<PdfElement>(context.PageData.Elements);

        SKSize pageSize = SkiaUtils.GetSkPageSize(context.PageData.Size, context.PageData.Orientation);
        var pageMargins = context.PageData.Padding;
        var contentRect = new SKRect(
            (float)pageMargins.Left, (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right, pageSize.Height - (float)pageMargins.Bottom
        );

        while (elementsToProcess.Count > 0)
        {
            var currentPageLayouts = new List<LayoutInfo>();
            float currentY = contentRect.Top;

            while (elementsToProcess.Count > 0)
            {
                var element = elementsToProcess.Peek();
                var renderer = context.RendererFactory.GetRenderer(element);
                var availableRectForMeasure = new SKRect(contentRect.Left, currentY, contentRect.Right, contentRect.Bottom);
                var elementContext = context with { Element = element };

                var layoutInfo = await renderer.MeasureAsync(elementContext, availableRectForMeasure);

                bool fitsOnPage = layoutInfo.Height <= availableRectForMeasure.Height;
                bool isFirstElement = currentY == contentRect.Top;
                bool needsNewPage = !isFirstElement && !fitsOnPage;

                if (needsNewPage)
                {
                    break;
                }

                elementsToProcess.Dequeue();
                currentPageLayouts.Add(layoutInfo);
                currentY += layoutInfo.Height;

                if (elementsToProcess.Count > 0)
                {
                    currentY += context.PageData.PageDefaultSpacing;
                }

                if (layoutInfo.RemainingElement is not null)
                {
                    var tempList = new List<PdfElement> { layoutInfo.RemainingElement };
                    tempList.AddRange(elementsToProcess);
                    elementsToProcess = new Queue<PdfElement>(tempList);
                }
            }

            if (currentPageLayouts.Count > 0)
            {
                pageBlocks.Add(currentPageLayouts.AsReadOnly());
            }
            else if (elementsToProcess.Count > 0)
            {
                var element = elementsToProcess.Dequeue();
                context.Logger.LogWarning("An element was too large to fit on a page by itself and was skipped: {ElementType}", element.GetType().Name);
            }
        }

        return pageBlocks;
    }
}
