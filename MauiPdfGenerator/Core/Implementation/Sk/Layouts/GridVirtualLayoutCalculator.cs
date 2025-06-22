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
using System.Diagnostics;
using MauiPdfGenerator.Fluent.Models.Layouts;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Core.Implementation.Sk.Layouts;

/// <summary>
/// Calcula virtualmente el layout de un grid (ancho de columnas, alto de filas) antes del renderizado real.
/// </summary>
internal class GridVirtualLayoutCalculator
{
    public record struct GridLayoutResult(float[] ColumnWidths, float[] RowHeights);

    // Estructura para representar la información de cada celda
    private record CellInfo(PdfElement Element, int Row, int Column, GridUnitType RowType, double RowValue, GridUnitType ColType, double ColValue);

    public async Task<GridLayoutResult> MeasureAsync(
        PdfGrid grid,
        PdfPageData pageDef,
        Func<SKCanvas, PdfElement, PdfPageData, SKRect, float, Task<RenderOutput>> elementRenderer)
    {
        var colDefs = grid.ColumnDefinitionsList;
        var rowDefs = grid.RowDefinitionsList;
        int colCount = colDefs.Count;
        int rowCount = rowDefs.Count;
        float[] colWidths = new float[colCount];
        float[] rowHeights = new float[rowCount];

        using var dummyCanvas = new SKCanvas(new SKBitmap(1, 1));

        // Obtener tamaño de página
        var pageSize = Core.Implementation.Sk.SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        float pageWidth = pageSize.Width - (float)pageDef.Margins.HorizontalThickness;
        float pageHeight = pageSize.Height - (float)pageDef.Margins.VerticalThickness;

        // 1. Organizar la información de cada celda
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

        // 2. Calcular anchos de columnas absolutas y contar autos
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

        // 3. Medir celdas y calcular anchos de columnas auto
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
                        // Ajuste MAUI-like: respetar HorizontalOptions/VerticalOptions
                        var element = cell.Element;
                        float widthForMeasure = element.GetHorizontalOptions == LayoutAlignment.Fill ? availableWidth : float.MaxValue;
                        float heightForMeasure = element.GetVerticalOptions == LayoutAlignment.Fill ? availableHeight : float.MaxValue;
                        // Validación especial para imágenes en auto/auto
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
                        // Mensaje informativo: ancho ocupado por el elemento en la celda
                        System.Diagnostics.Debug.WriteLine($"[INFO] Elemento tipo {element.GetType().Name} en celda [{r},{c}] ocupa ancho: {result.WidthDrawnThisCall}");
                    }
                }
                maxAutoColWidths[c] = maxWidth;
            }
        }
        // Repartir el espacio disponible entre columnas auto según su requerimiento, pero sin exceder el total disponible
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

        // 4. Calcular altos de filas absolutas y contar autos
        float totalAbsoluteRowHeight = 0;
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
        }
        float availableHeightForAutoRows = Math.Max(0, pageHeight - totalAbsoluteRowHeight);

        // 5. Medir celdas y calcular altos de filas auto
        float[] maxAutoRowHeights = new float[rowCount];
        for (int r = 0; r < rowCount; r++)
        {
            var rowDef = rowDefs[r];
            if (rowDef.GridUnitType == GridUnitType.Auto)
            {
                float maxHeight = 0;
                for (int c = 0; c < colCount; c++)
                {
                    var cell = cellInfos.FirstOrDefault(ci => ci.Row == r && ci.Column == c);
                    if (cell != null)
                    {
                        float availableWidth = colDefs[c].GridUnitType == GridUnitType.Absolute ? (float)colDefs[c].Value : colWidths[c];
                        float availableHeight = availableHeightForAutoRows;
                        // Ajuste MAUI-like: respetar HorizontalOptions/VerticalOptions
                        var element = cell.Element;
                        float widthForMeasure = element.GetHorizontalOptions == LayoutAlignment.Fill ? availableWidth : float.MaxValue;
                        float heightForMeasure = element.GetVerticalOptions == LayoutAlignment.Fill ? availableHeight : float.MaxValue;
                        // Validación especial para imágenes en auto/auto
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
                maxAutoRowHeights[r] = maxHeight;
            }
        }
        // Repartir el espacio disponible entre filas auto según su requerimiento, pero sin exceder el total disponible
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

        return new GridLayoutResult(colWidths, rowHeights);
    }
}
