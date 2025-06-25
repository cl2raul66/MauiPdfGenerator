using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using SkiaSharp;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class LayoutRenderer
{
    private static readonly Dictionary<PdfGrid, float> _gridLeftPositions = [];

    private async Task<Dictionary<PdfElement, RenderOutput>> MeasureAndPrerenderChildren(IEnumerable<PdfElement> children, SKRect availableRect, PdfPageData pageDef, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var result = new Dictionary<PdfElement, RenderOutput>();
        using var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));
        foreach (var child in children)
        {
            var measure = await elementRenderer(dummyCanvas, child, pageDef, availableRect, 0);
            result[child] = measure;
        }
        return result;
    }

    public async Task<RenderOutput> RenderPdfContentPageAutoAsync(
        SKCanvas canvas,
        PdfPageData pageDef,
        SKRect contentRect,
        Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, PdfFontRegistryBuilder, Task<RenderOutput>> elementRenderer,
        PdfFontRegistryBuilder fontRegistry)
    {
        return await RenderPdfContentPageAsLayoutAsync(canvas, pageDef, contentRect, elementRenderer, fontRegistry);
    }

    public async Task<RenderOutput> RenderGridAsLayoutAsync(SKCanvas canvas, PdfGrid grid, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var allChildren = grid.GetChildren;
        var availableRect = SKRect.Create(0, 0, parentRect.Width, parentRect.Height);
        var childMeasures = await MeasureAndPrerenderChildren(allChildren, availableRect, pageDef, elementRenderer);
        var layoutCalculator = new GridVirtualLayoutCalculator();
        var layoutResult = await layoutCalculator.MeasureAsync(grid, pageDef, elementRenderer);
        float[] colWidths = layoutResult.ColumnWidths;
        float[] rowHeights = layoutResult.RowHeights;
        float gridWidth = colWidths.Sum() + grid.GetSpacing * (colWidths.Length - 1);
        float gridHeight = rowHeights.Sum() + grid.GetSpacing * (rowHeights.Length - 1);
        float left = parentRect.Left + (float)grid.GetMargin.Left + (float)grid.GetPadding.Left;
        float top = startY + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top;
        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            SKRect bgRect = SKRect.Create(left, top, gridWidth, gridHeight);
            canvas.DrawRect(bgRect, bgPaint);
        }
        float y = top;
        for (int r = 0; r < rowHeights.Length; r++)
        {
            float x = left;
            for (int c = 0; c < colWidths.Length; c++)
            {
                var childrenInCell = grid.GetChildren.Where(e => e.GridRow == r && e.GridColumn == c).ToList();
                if (childrenInCell.Count == 0) { x += colWidths[c] + grid.GetSpacing; continue; }
                float cellWidth = colWidths[c];
                float cellHeight = rowHeights[r];
                foreach (var child in childrenInCell)
                {
                    var measure = childMeasures[child];
                    float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? cellWidth : measure.WidthDrawnThisCall;
                    float childHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? cellHeight : measure.VisualHeightDrawn;
                    float offsetX = child.GetHorizontalOptions switch
                    {
                        LayoutAlignment.Center => (cellWidth - childWidth) / 2f,
                        LayoutAlignment.End => cellWidth - childWidth,
                        _ => 0f
                    };
                    float offsetY = child.GetVerticalOptions switch
                    {
                        LayoutAlignment.Center => (cellHeight - childHeight) / 2f,
                        LayoutAlignment.End => cellHeight - childHeight,
                        _ => 0f
                    };
                    var childRect = SKRect.Create(x + offsetX, y + offsetY, childWidth, childHeight);
                    await elementRenderer(canvas, child, pageDef, childRect, childRect.Top);
                }
                x += colWidths[c] + grid.GetSpacing;
            }
            y += rowHeights[r] + grid.GetSpacing;
        }
        float totalHeight = gridHeight + (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;
        float totalWidth = gridWidth + (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;
        return new RenderOutput(totalHeight, totalWidth, null, false);
    }

    public async Task<RenderOutput> RenderVerticalStackLayoutAsync(SKCanvas canvas, PdfVerticalStackLayout vsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var allChildren = vsl.GetChildren;
        var availableRect = SKRect.Create(0, 0, parentRect.Width, parentRect.Height);
        var childMeasures = await MeasureAndPrerenderChildren(allChildren, availableRect, pageDef, elementRenderer);
        float leftMargin = (float)vsl.GetMargin.Left;
        float rightMargin = (float)vsl.GetMargin.Right;
        SKRect vslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);
        if (vsl.GetWidthRequest.HasValue && vsl.GetWidthRequest > 0)
        {
            vslMarginRect.Right = vslMarginRect.Left + (float)vsl.GetWidthRequest.Value;
        }
        if (vslMarginRect.Width <= 0 || vslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }
        SKRect vslPaddedContentRect = new(
            0, 0,
            vslMarginRect.Width - (float)vsl.GetPadding.HorizontalThickness,
            vslMarginRect.Height - (float)vsl.GetPadding.VerticalThickness
        );
        float currentYinVsl = 0;
        float totalContentHeightDrawn = 0;
        float totalContentWidthDrawn = 0;
        var childrenToRender = new Queue<PdfElement>(vsl.GetChildren);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;
        while (childrenToRender.Count != 0)
        {
            var child = childrenToRender.Dequeue();
            var measure = childMeasures[child];
            float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? vslPaddedContentRect.Width : measure.WidthDrawnThisCall;
            float childHeight = child.GetHeightRequest.HasValue ? (float)child.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (vslPaddedContentRect.Width - childWidth) / 2f,
                LayoutAlignment.End => vslPaddedContentRect.Width - childWidth,
                _ => 0f
            };
            var childAvailableRect = SKRect.Create(vslMarginRect.Left + (float)vsl.GetPadding.Left + offsetX, startY + (float)vsl.GetPadding.Top + currentYinVsl, childWidth, childHeight);
            await elementRenderer(canvas, child, pageDef, childAvailableRect, childAvailableRect.Top);
            float heightToAdvance = measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall;
            if (heightToAdvance > 0)
            {
                currentYinVsl += heightToAdvance;
                totalContentHeightDrawn += heightToAdvance;
                totalContentWidthDrawn = Math.Max(totalContentWidthDrawn, measure.WidthDrawnThisCall);
            }
            if (childrenToRender.Count != 0)
            {
                currentYinVsl += vsl.GetSpacing;
                totalContentHeightDrawn += vsl.GetSpacing;
            }
        }
        float visualContentHeight = totalContentHeightDrawn;
        float finalLayoutHeight = vsl.GetHeightRequest.HasValue && vsl.GetHeightRequest > 0 ?
            (float)vsl.GetHeightRequest.Value :
            visualContentHeight + (float)vsl.GetPadding.VerticalThickness;
        float naturalLayoutWidth = totalContentWidthDrawn + (float)vsl.GetPadding.HorizontalThickness;
        if (totalContentHeightDrawn > 0)
        {
            float finalContainerWidth = vsl.GetHorizontalOptions is LayoutAlignment.Fill ? vslMarginRect.Width : naturalLayoutWidth;
            if (vsl.GetWidthRequest.HasValue && vsl.GetWidthRequest > 0) finalContainerWidth = (float)vsl.GetWidthRequest.Value;
            float offsetX = vsl.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (vslMarginRect.Width - finalContainerWidth) / 2,
                LayoutAlignment.End => vslMarginRect.Width - finalContainerWidth,
                _ => 0,
            };
            float finalX = vslMarginRect.Left + offsetX;
            if (vsl.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalLayoutHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }
        }
        float consumedWidth = vsl.GetWidthRequest.HasValue && vsl.GetWidthRequest > 0 ? (float)vsl.GetWidthRequest.Value : naturalLayoutWidth;
        return new RenderOutput(finalLayoutHeight, consumedWidth, null, requiresNewPage, visualContentHeight);
    }

    public async Task<RenderOutput> RenderHorizontalStackLayoutAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var allChildren = hsl.GetChildren;
        var availableRect = SKRect.Create(0, 0, parentRect.Width, parentRect.Height);
        var childMeasures = await MeasureAndPrerenderChildren(allChildren, availableRect, pageDef, elementRenderer);
        float leftMargin = (float)hsl.GetMargin.Left;
        float rightMargin = (float)hsl.GetMargin.Right;
        SKRect hslMarginRect = SKRect.Create(parentRect.Left + leftMargin, startY, parentRect.Width - leftMargin - rightMargin, parentRect.Bottom - startY);
        if (hsl.GetWidthRequest.HasValue && hsl.GetWidthRequest > 0)
        {
            hslMarginRect.Right = hslMarginRect.Left + (float)hsl.GetWidthRequest.Value;
        }
        if (hslMarginRect.Width <= 0 || hslMarginRect.Height <= 0)
        {
            return new RenderOutput(0, 0, null, false);
        }
        SKRect hslPaddedContentRect = new(
            0, 0,
            hslMarginRect.Width - (float)hsl.GetPadding.HorizontalThickness,
            hslMarginRect.Height - (float)hsl.GetPadding.VerticalThickness
        );
        float currentXinHsl = 0;
        float maxChildContentHeight = 0;
        var childrenToRender = new Queue<PdfElement>(hsl.GetChildren);
        while (childrenToRender.Count != 0)
        {
            var child = childrenToRender.Dequeue();
            var measure = childMeasures[child];
            float childWidth = child.GetWidthRequest.HasValue ? (float)child.GetWidthRequest.Value : measure.WidthDrawnThisCall;
            float childHeight = child.GetHeightRequest.HasValue ? (float)child.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
            var childAvailableRect = SKRect.Create(currentXinHsl, 0, childWidth, hslPaddedContentRect.Height);
            await elementRenderer(canvas, child, pageDef, childAvailableRect, 0);
            if (childHeight > 0)
            {
                maxChildContentHeight = Math.Max(maxChildContentHeight, childHeight);
                currentXinHsl += childWidth;
            }
            if (childrenToRender.Count != 0)
            {
                currentXinHsl += hsl.GetSpacing;
            }
        }
        float naturalContentWidth = currentXinHsl;
        float layoutContentHeight = maxChildContentHeight;
        float finalVisualHeight = hsl.GetHeightRequest.HasValue && hsl.GetHeightRequest > 0 ?
            (float)hsl.GetHeightRequest.Value :
            layoutContentHeight + (float)hsl.GetPadding.VerticalThickness;
        float naturalLayoutWidth = naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness;
        float finalContainerWidth = hsl.GetHorizontalOptions == LayoutAlignment.Fill ? hslMarginRect.Width : naturalLayoutWidth;
        if (hsl.GetWidthRequest.HasValue && hsl.GetWidthRequest > 0) finalContainerWidth = (float)hsl.GetWidthRequest.Value;
        float offsetX = hsl.GetHorizontalOptions switch
        {
            LayoutAlignment.Center => (hslMarginRect.Width - finalContainerWidth) / 2,
            LayoutAlignment.End => hslMarginRect.Width - finalContainerWidth,
            _ => 0
        };
        float finalX = hslMarginRect.Left + offsetX;
        float availableContentHeight = finalVisualHeight - (float)hsl.GetPadding.VerticalThickness;
        float contentOffsetY = hsl.GetVerticalOptions switch
        {
            LayoutAlignment.Center => (availableContentHeight - layoutContentHeight) / 2,
            LayoutAlignment.End => availableContentHeight - layoutContentHeight,
            _ => 0,
        };
        if (hsl.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(hsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
            SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalVisualHeight);
            canvas.DrawRect(bgRect, bgPaint);
        }
        canvas.Save();
        canvas.Translate(finalX + (float)hsl.GetPadding.Left, startY + (float)hsl.GetPadding.Top + contentOffsetY);
        float renderX = 0;
        foreach (var child in hsl.GetChildren)
        {
            var measure = childMeasures[child];
            float childWidth = child.GetWidthRequest.HasValue ? (float)child.GetWidthRequest.Value : measure.WidthDrawnThisCall;
            float childHeight = child.GetHeightRequest.HasValue ? (float)child.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
            var childRect = SKRect.Create(renderX, 0, childWidth, childHeight);
            await elementRenderer(canvas, child, pageDef, childRect, 0);
            renderX += childWidth + hsl.GetSpacing;
        }
        canvas.Restore();
        float consumedWidth = hsl.GetWidthRequest.HasValue && hsl.GetWidthRequest > 0 ? (float)hsl.GetWidthRequest.Value : naturalLayoutWidth;
        return new RenderOutput(finalVisualHeight, consumedWidth, null, false, layoutContentHeight);
    }

    public async Task<RenderOutput> RenderPdfContentPageAsLayoutAsync(
        SKCanvas canvas,
        PdfPageData pageDef,
        SKRect contentRect,
        Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, PdfFontRegistryBuilder, Task<RenderOutput>> elementRenderer,
        PdfFontRegistryBuilder fontRegistry)
    {
        float currentY = contentRect.Top;
        float maxWidth = 0;
        foreach (var element in pageDef.Elements)
        {
            var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));
            var measure = await elementRenderer(dummyCanvas, element, pageDef, contentRect, currentY, fontRegistry);
            float elementHeight = element.GetHeightRequest.HasValue ? (float)element.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
            float elementWidth = element.GetWidthRequest.HasValue ? (float)element.GetWidthRequest.Value : measure.WidthDrawnThisCall;
            float offsetX = element.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentRect.Width - elementWidth) / 2f,
                LayoutAlignment.End => contentRect.Width - elementWidth,
                _ => 0f
            };
            var elementRect = SKRect.Create(contentRect.Left + offsetX, currentY, elementWidth, elementHeight);
            await elementRenderer(canvas, element, pageDef, elementRect, currentY, fontRegistry);
            currentY += elementHeight + pageDef.PageDefaultSpacing;
            maxWidth = Math.Max(maxWidth, elementWidth);
        }
        float totalHeight = currentY - contentRect.Top;
        return new RenderOutput(totalHeight, maxWidth, null, false, totalHeight);
    }

    public async Task RenderPdfContentPageWithPaginationAsync(
        SKDocument pdfDoc,
        PdfPageData pageDef,
        SKSize pageSize,
        PdfFontRegistryBuilder fontRegistry,
        Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, PdfFontRegistryBuilder, Task<RenderOutput>> elementRenderer)
    {
        var pageMargins = pageDef.Margins;
        var contentRect = new SKRect(
            (float)pageMargins.Left,
            (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right,
            pageSize.Height - (float)pageMargins.Bottom
        );
        int pageNumber = 1;
        int elementIndex = 0;
        var elements = pageDef.Elements.ToList();
        while (elementIndex < elements.Count)
        {
            using var canvas = pdfDoc.BeginPage(pageSize.Width, pageSize.Height);
            canvas.Clear(pageDef.BackgroundColor is not null
                ? SkiaUtils.ConvertToSkColor(pageDef.BackgroundColor)
                : SKColors.White);
            float currentY = contentRect.Top;
            float maxWidth = 0;
            while (elementIndex < elements.Count)
            {
                var element = elements[elementIndex];
                var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));
                var measure = await elementRenderer(dummyCanvas, element, pageDef, contentRect, currentY, fontRegistry);
                float elementHeight = element.GetHeightRequest.HasValue ? (float)element.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
                float elementWidth = element.GetWidthRequest.HasValue ? (float)element.GetWidthRequest.Value : measure.WidthDrawnThisCall;
                float spacing = (elementIndex < elements.Count - 1) ? pageDef.PageDefaultSpacing : 0f;
                if (currentY + elementHeight + spacing > contentRect.Bottom + 0.01f)
                {
                    // Si el elemento no cabe, pero es divisible (tiene RemainingElement), renderiza la parte que cabe y deja el resto para la siguiente página
                    if (measure.RemainingElement is not null)
                    {
                        // Renderiza la parte que cabe
                        var elementRect = SKRect.Create(contentRect.Left, currentY, elementWidth, elementHeight);
                        await elementRenderer(canvas, element, pageDef, elementRect, currentY, fontRegistry);
                        // Sustituye el elemento actual por el fragmento restante
                        elements[elementIndex] = measure.RemainingElement;
                        break; // Salta a la siguiente página
                    }
                    else if (currentY == contentRect.Top)
                    {
                        // Si el elemento es indivisible y no cabe ni siquiera en una página vacía, lo renderiza igual (para evitar bucle infinito)
                        var elementRect = SKRect.Create(contentRect.Left, currentY, elementWidth, elementHeight);
                        await elementRenderer(canvas, element, pageDef, elementRect, currentY, fontRegistry);
                        elementIndex++;
                        break;
                    }
                    else
                    {
                        // Salta a la siguiente página sin avanzar el índice
                        break;
                    }
                }
                else
                {
                    var elementRect = SKRect.Create(contentRect.Left, currentY, elementWidth, elementHeight);
                    await elementRenderer(canvas, element, pageDef, elementRect, currentY, fontRegistry);
                    currentY += elementHeight + spacing;
                    maxWidth = Math.Max(maxWidth, elementWidth);
                    elementIndex++;
                }
            }
            System.Diagnostics.Debug.WriteLine($"[PDF-PAGINATE] Página física: {pageNumber} Tamaño: {pageSize.Width}x{pageSize.Height} Área render: [{contentRect.Left},{contentRect.Top},{contentRect.Width},{contentRect.Height}]");
            pageNumber++;
            pdfDoc.EndPage();
        }
    }
}
