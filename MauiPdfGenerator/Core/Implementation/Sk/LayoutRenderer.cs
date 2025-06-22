// Ignore Spelling: vsl hsl

using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using SkiaSharp;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class LayoutRenderer
{
    public async Task<RenderOutput> RenderGridAsLayoutAsync(SKCanvas canvas, PdfGrid grid, PdfPageData pageDef, SKRect parentRect, float startY, Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        // Detectar fase de medición o renderizado
        bool isMeasurePhase = canvas.DeviceClipBounds.Width == 1 && canvas.DeviceClipBounds.Height == 1;
        string phase = isMeasurePhase ? "[MEASURE]" : "[DRAW]";

        // Usar el nuevo GridVirtualLayoutCalculator
        var layoutCalculator = new GridVirtualLayoutCalculator();
        var layoutResult = await layoutCalculator.MeasureAsync(grid, pageDef, elementRenderer);
        float[] colWidths = layoutResult.ColumnWidths;
        float[] rowHeights = layoutResult.RowHeights;
        var colDefs = grid.ColumnDefinitionsList;
        var rowDefs = grid.RowDefinitionsList;

        // Calcular el área real del grid dentro del parentRect
        float gridWidth = colWidths.Sum();
        float gridHeight = rowHeights.Sum();
        float totalHorizontal = (float)grid.GetMargin.Left + (float)grid.GetMargin.Right + (float)grid.GetPadding.Left + (float)grid.GetPadding.Right;

        // Depuración: área disponible y ancho del grid
        System.Diagnostics.Debug.WriteLine($"[GRID-DEBUG]{phase} parentRect: Left={parentRect.Left}, Top={parentRect.Top}, Width={parentRect.Width}, Height={parentRect.Height}, gridWidth={gridWidth}, gridHeight={gridHeight}");

        float left;
        // Solo aplicar alineación si el área disponible es mayor que el grid
        if (parentRect.Width > gridWidth)
        {
            if (grid.GetHorizontalOptions == LayoutAlignment.Center)
            {
                left = parentRect.Left + (parentRect.Width - gridWidth) / 2f;
            }
            else if (grid.GetHorizontalOptions == LayoutAlignment.End)
            {
                left = parentRect.Left + (parentRect.Width - gridWidth);
            }
            else
            {
                left = parentRect.Left;
            }
        }
        else
        {
            // Si el área disponible es igual al grid, no aplicar alineación
            left = parentRect.Left;
        }
        left += (float)grid.GetMargin.Left + (float)grid.GetPadding.Left;
        float top = startY + (float)grid.GetMargin.Top + (float)grid.GetPadding.Top;

        // Depuración: posición final donde se dibuja el grid
        System.Diagnostics.Debug.WriteLine($"[GRID-DEBUG]{phase} Grid se dibuja en: Left={left}, Top={top}, Width={gridWidth}, Height={gridHeight}");

        // Dibuja el fondo del grid si corresponde
        if (grid.GetBackgroundColor is not null)
        {
            using var bgPaint = new SKPaint { Color = SkiaUtils.ConvertToSkColor(grid.GetBackgroundColor), Style = SKPaintStyle.Fill };
            SKRect bgRect = SKRect.Create(left, top, gridWidth, gridHeight);
            canvas.DrawRect(bgRect, bgPaint);
        }

        // Renderizado manual de celdas para respetar HorizontalOptions/VerticalOptions
        float y = top;
        for (int r = 0; r < rowHeights.Length; r++)
        {
            float x = left;
            for (int c = 0; c < colWidths.Length; c++)
            {
                var child = grid.GetChildren.FirstOrDefault(e => e.GridRow == r && e.GridColumn == c);
                if (child == null) { x += colWidths[c]; continue; }
                float cellWidth = colWidths[c];
                float cellHeight = rowHeights[r];
                // Medir el contenido del hijo para saber su tamaño natural
                var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));
                var measure = await elementRenderer(dummyCanvas, child, pageDef, new SKRect(0, 0, cellWidth, cellHeight), 0);
                float childWidth = child.GetHorizontalOptions == LayoutAlignment.Fill ? cellWidth : measure.WidthDrawnThisCall;
                float childHeight = child.GetVerticalOptions == LayoutAlignment.Fill ? cellHeight : measure.VisualHeightDrawn;
                // Calcular alineación
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
                x += colWidths[c];
            }
            y += rowHeights[r];
        }
        float totalHeight = gridHeight + (float)grid.GetPadding.VerticalThickness + (float)grid.GetMargin.VerticalThickness;
        float totalWidth = gridWidth + (float)grid.GetPadding.HorizontalThickness + (float)grid.GetMargin.HorizontalThickness;
        return new RenderOutput(totalHeight, totalWidth, null, false);
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

        float currentYinVsl = 0;
        float totalContentHeightDrawn = 0;
        float totalContentWidthDrawn = 0;
        float previousElementBottom = 0;
        int elementIndex = 0;

        var childrenToRender = new Queue<PdfElement>(vsl.GetChildren);
        var remainingChildrenForNextPage = new List<PdfElement>();
        bool requiresNewPage = false;

        while (childrenToRender.Count != 0)
        {
            var child = childrenToRender.Dequeue();
            // Medir el ancho natural del hijo
            var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));
            var measure = await elementRenderer(dummyCanvas, child, pageDef, SKRect.Create(0, 0, vslPaddedContentRect.Width, vslPaddedContentRect.Height - currentYinVsl), 0);
            float childWidth;
            if (child.GetHorizontalOptions == LayoutAlignment.Fill)
                childWidth = vslPaddedContentRect.Width;
            else if (child.GetHorizontalOptions == LayoutAlignment.Center)
                childWidth = measure.WidthDrawnThisCall;
            else
                childWidth = measure.WidthDrawnThisCall;

            float childHeight = child.GetHeightRequest.HasValue ? (float)child.GetHeightRequest.Value : (measure.VisualHeightDrawn > 0 ? measure.VisualHeightDrawn : measure.HeightDrawnThisCall);
            float offsetX = child.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (vslPaddedContentRect.Width - childWidth) / 2f,
                LayoutAlignment.End => vslPaddedContentRect.Width - childWidth,
                _ => 0f
            };
            var childAvailableRect = SKRect.Create(vslMarginRect.Left + (float)vsl.GetPadding.Left + offsetX, startY + (float)vsl.GetPadding.Top + currentYinVsl, childWidth, childHeight);
            var result = await elementRenderer(canvas, child, pageDef, childAvailableRect, childAvailableRect.Top);

            // Usar VisualHeightDrawn si es mayor que cero, para imágenes y otros elementos
            float heightToAdvance = result.VisualHeightDrawn > 0 ? result.VisualHeightDrawn : result.HeightDrawnThisCall;
            if (heightToAdvance > 0)
            {
                // Mensaje de depuración: diferencia vertical con el elemento anterior
                if (elementIndex > 0)
                {
                    float espacioArriba = childAvailableRect.Top - previousElementBottom;
                    System.Diagnostics.Debug.WriteLine($"[INFO] Espacio arriba del elemento {elementIndex} ({child.GetType().Name}): {espacioArriba}");
                }
                previousElementBottom = childAvailableRect.Top + heightToAdvance;
                elementIndex++;
                currentYinVsl += heightToAdvance;
                totalContentHeightDrawn += heightToAdvance;
                totalContentWidthDrawn = Math.Max(totalContentWidthDrawn, result.WidthDrawnThisCall);
                System.Diagnostics.Debug.WriteLine($"[INFO] Elemento tipo {child.GetType().Name} ocupa ancho: {result.WidthDrawnThisCall} y alto: {heightToAdvance}");
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

        while (childrenToRender.Count != 0)
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

    // Renderiza los elementos directos de una página de contenido (flujo vertical)
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
            // Medir el elemento para obtener su altura
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
}
