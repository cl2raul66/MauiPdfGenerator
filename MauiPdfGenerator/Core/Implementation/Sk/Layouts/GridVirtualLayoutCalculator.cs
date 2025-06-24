using Microsoft.Maui.Controls;
using Microsoft.Maui;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Core.Models;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using MauiPdfGenerator.Fluent.Models.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

internal class GridVirtualLayoutCalculator
{
    public record struct GridLayoutResult(float[] ColumnWidths, float[] RowHeights);
    private record CellInfo(PdfElement Element, int Row, int Column, GridUnitType RowType, double RowValue, GridUnitType ColType, double ColValue);

    public async Task<GridLayoutResult> MeasureAsync(
        PdfGrid grid,
        PdfPageData pageDef,
        Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        System.Diagnostics.Debug.WriteLine($"[GRID-PRERENDER] Grid con {grid.GetChildren.Count} hijos, filas: {grid.RowDefinitionsList.Count}, columnas: {grid.ColumnDefinitionsList.Count}");

        var colDefs = grid.ColumnDefinitionsList;
        var rowDefs = grid.RowDefinitionsList;
        int colCount = colDefs.Count;
        int rowCount = rowDefs.Count;
        float[] colWidths = new float[colCount];
        float[] rowHeights = new float[rowCount];

        using var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));

        var pageSize = Core.Implementation.Sk.SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        float pageWidth = pageSize.Width - (float)pageDef.Margins.HorizontalThickness;
        float pageHeight = pageSize.Height - (float)pageDef.Margins.VerticalThickness;

        var cellInfos = new List<CellInfo>();
        foreach (var child in grid.GetChildren.Where(e => e.GridRowSpan == 1 && e.GridColumnSpan == 1))
        {
            int r = child.GridRow;
            int c = child.GridColumn;
            var rowDef = rowDefs[r];
            var colDef = colDefs[c];
            cellInfos.Add(new CellInfo(
                child,
                r,
                c,
                rowDef.GridUnitType,
                rowDef.Value,
                colDef.GridUnitType,
                colDef.Value
            ));
        }

        float totalAbsoluteColWidth = 0;
        int autoColCount = 0;
        for (int c = 0; c < colCount; c++)
        {
            var colDef = colDefs[c];
            if (colDef.GridUnitType == GridUnitType.Absolute)
            {
                colWidths[c] = (float)colDef.Value;
                totalAbsoluteColWidth += (float)colDef.Value;
            }
            else if (colDef.GridUnitType == GridUnitType.Auto)
            {
                autoColCount++;
            }
        }
        float availableWidthForAutoCols = Math.Max(0, pageWidth - totalAbsoluteColWidth);

        var cellMeasures = new RenderOutput[rowCount, colCount];
        float[] maxAutoColWidths = new float[colCount];
        for (int c = 0; c < colCount; c++)
        {
            var colDef = colDefs[c];
            if (colDef.GridUnitType == GridUnitType.Auto)
            {
                float maxWidth = 0;
                for (int r = 0; r < rowCount; r++)
                {
                    var cell = cellInfos.FirstOrDefault(ci => ci.Row == r && ci.Column == c);
                    if (cell != null)
                    {
                        float availableWidth = availableWidthForAutoCols;
                        float availableHeight = cell.RowType == GridUnitType.Absolute ? (float)cell.RowValue : pageHeight;
                        var element = cell.Element;
                        float widthForMeasure = element.GetHorizontalOptions == LayoutAlignment.Fill ? availableWidth : float.MaxValue;
                        float heightForMeasure = element.GetVerticalOptions == LayoutAlignment.Fill ? availableHeight : float.MaxValue;
                        if (element is PdfImage img && cell.ColType == GridUnitType.Auto && cell.RowType == GridUnitType.Auto)
                        {
                            if (!img.GetWidthRequest.HasValue && !img.GetHeightRequest.HasValue)
                            {
                                throw new InvalidOperationException($"La imagen en la celda [{cell.Row},{cell.Column}] con columnas y filas auto debe tener WidthRequest o HeightRequest definido.");
                            }
                        }
                        var result = await elementRenderer(dummyCanvas, element, pageDef, new SKRect(0, 0, widthForMeasure, heightForMeasure), 0);
                        cellMeasures[r, c] = result;
                        maxWidth = Math.Max(maxWidth, result.WidthDrawnThisCall);
                    }
                }
                maxAutoColWidths[c] = maxWidth;
            }
        }
        float totalAutoColWidth = maxAutoColWidths.Sum();
        for (int c = 0; c < colCount; c++)
        {
            if (colDefs[c].GridUnitType == GridUnitType.Auto)
            {
                if (totalAutoColWidth > 0)
                    colWidths[c] = Math.Min(maxAutoColWidths[c], availableWidthForAutoCols * (maxAutoColWidths[c] / totalAutoColWidth));
                else
                    colWidths[c] = availableWidthForAutoCols / autoColCount;
            }
        }

        float totalStarColValue = 0;
        for (int c = 0; c < colCount; c++)
        {
            if (colDefs[c].GridUnitType == GridUnitType.Star)
                totalStarColValue += (float)colDefs[c].Value;
        }
        float usedWidth = totalAbsoluteColWidth + totalAutoColWidth;
        float availableWidthForStarCols = Math.Min(pageWidth - usedWidth, Math.Max(0, pageWidth - usedWidth));
        for (int c = 0; c < colCount; c++)
        {
            if (colDefs[c].GridUnitType == GridUnitType.Star && totalStarColValue > 0)
                colWidths[c] = availableWidthForStarCols * ((float)colDefs[c].Value / totalStarColValue);
        }

        float totalAbsoluteRowHeight = 0;
        float totalStarRowValue = 0;
        int autoRowCount = 0;
        for (int r = 0; r < rowCount; r++)
        {
            var rowDef = rowDefs[r];
            if (rowDef.GridUnitType == GridUnitType.Absolute)
            {
                rowHeights[r] = (float)rowDef.Value;
                totalAbsoluteRowHeight += (float)rowDef.Value;
            }
            else if (rowDef.GridUnitType == GridUnitType.Auto)
            {
                autoRowCount++;
            }
            else if (rowDef.GridUnitType == GridUnitType.Star)
            {
                totalStarRowValue += (float)rowDef.Value;
            }
        }
        float availableHeightForAutoRows = Math.Max(0, pageHeight - totalAbsoluteRowHeight);

        float[] maxAutoRowHeights = new float[rowCount];
        for (int r = 0; r < rowCount; r++)
        {
            var rowDef = rowDefs[r];
            if (rowDef.GridUnitType == GridUnitType.Auto)
            {
                float maxHeight = 0;
                bool hasContent = false;
                for (int c = 0; c < colCount; c++)
                {
                    var cell = cellInfos.FirstOrDefault(ci => ci.Row == r && ci.Column == c);
                    if (cell != null)
                    {
                        hasContent = true;
                        float availableWidth = colDefs[c].GridUnitType == GridUnitType.Absolute ? (float)colDefs[c].Value : colWidths[c];
                        // CORRECCIÓN: Usar availableHeightForAutoRows en vez de float.MaxValue
                        float availableHeight = availableHeightForAutoRows > 0 ? availableHeightForAutoRows : pageHeight;
                        var element = cell.Element;
                        float widthForMeasure = element.GetHorizontalOptions == LayoutAlignment.Fill ? availableWidth : float.MaxValue;
                        float heightForMeasure = element.GetVerticalOptions == LayoutAlignment.Fill ? availableHeight : availableHeight;
                        if (element is PdfImage img && cell.ColType == GridUnitType.Auto && cell.RowType == GridUnitType.Auto)
                        {
                            if (!img.GetWidthRequest.HasValue && !img.GetHeightRequest.HasValue)
                            {
                                throw new InvalidOperationException($"La imagen en la celda [{cell.Row},{cell.Column}] con columnas y filas auto debe tener WidthRequest o HeightRequest definido.");
                            }
                        }
                        var result = cellMeasures[r, c];
                        if (result.Equals(default(RenderOutput)))
                        {
                            result = await elementRenderer(dummyCanvas, element, pageDef, new SKRect(0, 0, widthForMeasure, heightForMeasure), 0);
                            cellMeasures[r, c] = result;
                        }
                        maxHeight = Math.Max(maxHeight, result.VisualHeightDrawn);
                    }
                }
                if (hasContent && maxHeight <= 0)
                    maxHeight = 1;
                maxAutoRowHeights[r] = maxHeight;
            }
        }
        bool allRowsAuto = rowDefs.All(rd => rd.GridUnitType == GridUnitType.Auto);
        if (allRowsAuto) {
            for (int r = 0; r < rowCount; r++)
                rowHeights[r] = maxAutoRowHeights[r];
        } else {
            float totalAutoRowHeight = maxAutoRowHeights.Sum();
            for (int r = 0; r < rowCount; r++)
            {
                if (rowDefs[r].GridUnitType == GridUnitType.Auto)
                {
                    if (totalAutoRowHeight > 0)
                        rowHeights[r] = Math.Min(maxAutoRowHeights[r], availableHeightForAutoRows * (maxAutoRowHeights[r] / totalAutoRowHeight));
                    else
                        rowHeights[r] = availableHeightForAutoRows / autoRowCount;
                }
            }
        }

        float usedHeight = totalAbsoluteRowHeight + maxAutoRowHeights.Sum();
        float availableHeightForStarRows = Math.Min(pageHeight - usedHeight, Math.Max(0, pageHeight - usedHeight));
        for (int r = 0; r < rowCount; r++)
        {
            if (rowDefs[r].GridUnitType == GridUnitType.Star && totalStarRowValue > 0)
                rowHeights[r] = availableHeightForStarRows * ((float)rowDefs[r].Value / totalStarRowValue);
        }

        System.Diagnostics.Debug.WriteLine($"[GRID-POSTRENDER] Grid layout: colWidths=[{string.Join(",", colWidths)}], rowHeights=[{string.Join(",", rowHeights)}]");

        return new GridLayoutResult(colWidths, rowHeights);
    }
}
