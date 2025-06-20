// Ignore Spelling: vsl hsl

using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class LayoutRenderer
{
    public async Task<RenderOutput> RenderGridAsLayoutAsync(SKCanvas canvas, PdfGrid grid, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        // Fase 2: Lógica de Pre-medición para Auto
        var colWidths = await CalculateColumnWidths(grid, pageDef, elementRenderer);
        var rowHeights = await CalculateRowHeights(grid, colWidths, pageDef, elementRenderer);

        // Construcción de la estructura de Layouts anidados
        var gridRoot = new PdfVerticalStackLayout(null);
        gridRoot.Spacing(grid.GetSpacing);
        gridRoot.Margin(grid.GetMargin);
        gridRoot.Padding(grid.GetPadding);
        gridRoot.BackgroundColor(grid.GetBackgroundColor);
        gridRoot.HorizontalOptions(grid.GetHorizontalOptions);
        gridRoot.VerticalOptions(grid.GetVerticalOptions);
        if (grid.GetWidthRequest.HasValue) gridRoot.WidthRequest(grid.GetWidthRequest.Value);

        var childrenByRow = grid.GetChildren
            .OrderBy(c => c.GridColumn)
            .GroupBy(c => c.GridRow)
            .OrderBy(g => g.Key);

        for (int r = 0; r < rowHeights.Length; r++)
        {
            var rowLayout = new PdfHorizontalStackLayout(null);
            rowLayout.Spacing(grid.GetSpacing);
            rowLayout.HeightRequest(rowHeights[r]);

            var childrenInRow = childrenByRow.FirstOrDefault(g => g.Key == r);
            if (childrenInRow != null)
            {
                foreach (var child in childrenInRow)
                {
                    int colIndex = child.GridColumn;
                    if (colIndex < colWidths.Length)
                    {
                        child.WidthRequest(colWidths[colIndex]);
                    }
                    rowLayout.Add(child);
                }
            }
            gridRoot.Add(rowLayout);
        }

        return await RenderVerticalStackLayoutAsync(canvas, gridRoot, pageDef, parentRect, startY, elementRenderer);
    }

    private async Task<float[]> CalculateColumnWidths(PdfGrid grid, PdfPageData pageDef, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var colDefs = grid.ColumnDefinitionsList;
        var widths = new float[colDefs.Count];

        using var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));

        for (int i = 0; i < colDefs.Count; i++)
        {
            var colDef = colDefs[i];
            if (colDef.GridUnitType == GridUnitType.Absolute)
            {
                widths[i] = (float)colDef.Value;
            }
            else if (colDef.GridUnitType == GridUnitType.Auto)
            {
                var childrenInColumn = grid.GetChildren.Where(c => c.GridColumn == i && c.GridColumnSpan == 1);
                float maxWidth = 0;
                foreach (var child in childrenInColumn)
                {
                    var result = await elementRenderer(dummyCanvas, child, pageDef, new SKRect(0, 0, float.MaxValue, float.MaxValue), 0);
                    maxWidth = Math.Max(maxWidth, result.WidthDrawnThisCall);
                }
                widths[i] = maxWidth;
            }
        }
        return widths;
    }

    private async Task<float[]> CalculateRowHeights(PdfGrid grid, float[] colWidths, PdfPageData pageDef, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var rowDefs = grid.RowDefinitionsList;
        var heights = new float[rowDefs.Count];

        using var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));

        for (int i = 0; i < rowDefs.Count; i++)
        {
            var rowDef = rowDefs[i];
            if (rowDef.GridUnitType == GridUnitType.Absolute)
            {
                heights[i] = (float)rowDef.Value;
            }
            else if (rowDef.GridUnitType == GridUnitType.Auto)
            {
                var childrenInRow = grid.GetChildren.Where(c => c.GridRow == i && c.GridRowSpan == 1);
                float maxHeight = 0;
                foreach (var child in childrenInRow)
                {
                    float availableWidth = 0;
                    for (int j = 0; j < child.GridColumnSpan; j++)
                    {
                        if (child.GridColumn + j < colWidths.Length)
                        {
                            availableWidth += colWidths[child.GridColumn + j];
                        }
                    }
                    if (child.GridColumnSpan > 1)
                    {
                        availableWidth += grid.GetSpacing * (child.GridColumnSpan - 1);
                    }

                    var result = await elementRenderer(dummyCanvas, child, pageDef, new SKRect(0, 0, availableWidth, float.MaxValue), 0);
                    maxHeight = Math.Max(maxHeight, result.VisualHeightDrawn);
                }
                heights[i] = maxHeight;
            }
        }
        return heights;
    }

    public async Task<RenderOutput> RenderVerticalStackLayoutAsync(SKCanvas canvas, PdfVerticalStackLayout vsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
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

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(vslPaddedContentRect);

        float currentYinVsl = 0;
        float totalContentHeightDrawn = 0;
        float totalContentWidthDrawn = 0;

        var childrenToRender = new Queue<PdfElement>(vsl.GetChildren);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Count != 0)
        {
            var child = childrenToRender.Dequeue();
            var childAvailableRect = SKRect.Create(0, currentYinVsl, vslPaddedContentRect.Width, vslPaddedContentRect.Height - currentYinVsl);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, currentYinVsl);

            if (result.HeightDrawnThisCall > 0)
            {
                currentYinVsl += result.HeightDrawnThisCall;
                totalContentHeightDrawn += result.HeightDrawnThisCall;
                totalContentWidthDrawn = Math.Max(totalContentWidthDrawn, result.WidthDrawnThisCall);
            }

            if (result.RequiresNewPage || result.RemainingElement is not null)
            {
                requiresNewPage = true;
                if (result.RemainingElement is not null) remainingChildrenForNextPage.Add(result.RemainingElement);
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            if (childrenToRender.Count != 0)
            {
                if (currentYinVsl + vsl.GetSpacing > vslPaddedContentRect.Height + 0.01f)
                {
                    requiresNewPage = true;
                    remainingChildrenForNextPage.AddRange(childrenToRender);
                    break;
                }
                currentYinVsl += vsl.GetSpacing;
                totalContentHeightDrawn += vsl.GetSpacing;
            }
        }

        using SKPicture picture = recorder.EndRecording();

        float visualContentHeight = totalContentHeightDrawn;

        float finalLayoutHeight = vsl.GetHeightRequest.HasValue && vsl.GetHeightRequest > 0 ?
            (float)vsl.GetHeightRequest.Value :
            visualContentHeight + (float)vsl.GetPadding.VerticalThickness;

        float naturalLayoutWidth = totalContentWidthDrawn + (float)vsl.GetPadding.HorizontalThickness;

        if (totalContentHeightDrawn > 0)
        {
            float finalContainerWidth = vsl.GetHorizontalOptions is LayoutAlignment.Fill ? vslMarginRect.Width : naturalLayoutWidth;
            if (vsl.GetWidthRequest.HasValue && vsl.GetWidthRequest > 0) finalContainerWidth = (float)vsl.GetWidthRequest.Value;

            float offsetX = 0;

            switch (vsl.GetHorizontalOptions)
            {
                case LayoutAlignment.Center: offsetX = (vslMarginRect.Width - finalContainerWidth) / 2; break;
                case LayoutAlignment.End: offsetX = vslMarginRect.Width - finalContainerWidth; break;
                default: offsetX = 0; break;
            }

            float finalX = vslMarginRect.Left + offsetX;

            if (vsl.GetBackgroundColor is not null)
            {
                using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(vsl.GetBackgroundColor), Style = SKPaintStyle.Fill };
                SKRect bgRect = SKRect.Create(finalX, startY, finalContainerWidth, finalLayoutHeight);
                canvas.DrawRect(bgRect, bgPaint);
            }

            canvas.Save();
            canvas.Translate(finalX + (float)vsl.GetPadding.Left, startY + (float)vsl.GetPadding.Top);
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfVerticalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Count != 0)
        {
            continuation = new PdfVerticalStackLayout(remainingChildrenForNextPage, vsl);
        }

        float consumedWidth = vsl.GetWidthRequest.HasValue && vsl.GetWidthRequest > 0 ? (float)vsl.GetWidthRequest.Value : naturalLayoutWidth;
        return new RenderOutput(finalLayoutHeight, consumedWidth, continuation, requiresNewPage, visualContentHeight);
    }

    public async Task<RenderOutput> RenderHorizontalStackLayoutAsync(SKCanvas canvas, PdfHorizontalStackLayout hsl, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
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

        using var recorder = new SKPictureRecorder();
        using SKCanvas recordingCanvas = recorder.BeginRecording(hslPaddedContentRect);

        float currentXinHsl = 0;
        float maxChildContentHeight = 0;

        var childrenToRender = new Queue<PdfElement>(hsl.GetChildren);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Any())
        {
            var child = childrenToRender.Dequeue();

            if (currentXinHsl > 0 && (currentXinHsl + 1) > hslPaddedContentRect.Width)
            {
                requiresNewPage = true;
                remainingChildrenForNextPage.Add(child);
                remainingChildrenForNextPage.AddRange(childrenToRender);
                break;
            }

            var childAvailableRect = SKRect.Create(currentXinHsl, 0, hslPaddedContentRect.Width - currentXinHsl, hslPaddedContentRect.Height);
            var result = await elementRenderer(recordingCanvas, child, pageDef, childAvailableRect, 0);

            if (result.HeightDrawnThisCall > 0)
            {
                maxChildContentHeight = Math.Max(maxChildContentHeight, result.VisualHeightDrawn);
                currentXinHsl += result.WidthDrawnThisCall;
            }
            else if (child.GetWidthRequest.HasValue)
            {
                currentXinHsl += (float)child.GetWidthRequest.Value;
            }


            if (childrenToRender.Count != 0)
            {
                currentXinHsl += hsl.GetSpacing;
            }
        }

        float naturalContentWidth = currentXinHsl;

        using SKPicture picture = recorder.EndRecording();

        float layoutContentHeight = maxChildContentHeight;

        float finalVisualHeight = hsl.GetHeightRequest.HasValue && hsl.GetHeightRequest > 0 ?
            (float)hsl.GetHeightRequest.Value :
            layoutContentHeight + (float)hsl.GetPadding.VerticalThickness;

        if (finalVisualHeight > hslMarginRect.Height && finalVisualHeight > 0)
        {
            return new RenderOutput(0, 0, hsl, true);
        }

        if (layoutContentHeight > 0 || hsl.GetChildren.Any(c => c.GetHeightRequest.HasValue))
        {
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
            float contentOffsetY = 0;
            contentOffsetY = hsl.GetVerticalOptions switch
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
            canvas.DrawPicture(picture);
            canvas.Restore();
        }

        PdfHorizontalStackLayout? continuation = null;
        if (remainingChildrenForNextPage.Count != 0)
        {
            continuation = new PdfHorizontalStackLayout(remainingChildrenForNextPage, hsl);
        }

        float consumedWidth = hsl.GetWidthRequest.HasValue && hsl.GetWidthRequest > 0 ? (float)hsl.GetWidthRequest.Value : naturalContentWidth + (float)hsl.GetPadding.HorizontalThickness;
        return new RenderOutput(finalVisualHeight, consumedWidth, continuation, requiresNewPage, layoutContentHeight);
    }
}
