using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PageLayoutEngine
{
    private readonly ElementRendererFactory _rendererFactory = new();

    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(
        IReadOnlyList<PdfElement> elements,
        PdfPageData pageDef,
        Dictionary<PdfElement, object> layoutState,
        PdfFontRegistryBuilder fontRegistry)
    {
        var pageBlocks = new List<IReadOnlyList<LayoutInfo>>();
        var elementsToProcess = new Queue<PdfElement>(elements);

        SKSize pageSize = SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        var pageMargins = pageDef.Margins;
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
                var renderer = _rendererFactory.GetRenderer(element);
                var availableRectForMeasure = new SKRect(contentRect.Left, currentY, contentRect.Right, contentRect.Bottom);

                var layoutInfo = await renderer.MeasureAsync(element, _rendererFactory, pageDef, availableRectForMeasure, layoutState, fontRegistry);

                if (layoutInfo.Error is not null)
                {
                    elementsToProcess.Dequeue();
                    continue;
                }

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
                    currentY += pageDef.PageDefaultSpacing;
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
                var renderer = _rendererFactory.GetRenderer(element);
                var layoutInfo = await renderer.MeasureAsync(element, _rendererFactory, pageDef, contentRect, layoutState, fontRegistry);
                pageBlocks.Add(new List<LayoutInfo> { layoutInfo }.AsReadOnly());
            }
        }

        return pageBlocks;
    }
}
