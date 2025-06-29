using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender
{
    public Task<MeasureOutput> MeasureAsync(PdfHorizontalLine line, SKRect contentRect)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.HorizontalThickness;

        float heightRequired = thickness;
        float visualHeight = thickness;
        float widthRequired = lineContentWidth;

        bool requiresNewPage = thickness > contentRect.Height;

        var measureOutput = new MeasureOutput(heightRequired, visualHeight, widthRequired, [], null, requiresNewPage, 0, 0, 0, 0, null);
        return Task.FromResult(measureOutput);
    }

    public Task<RenderOutput> RenderAsync(SKCanvas canvas, PdfHorizontalLine line, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineContentX = contentRect.Left + (float)line.GetMargin.Left;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.Left - (float)line.GetMargin.Right;

        if (lineContentWidth <= 0)
        {
            return Task.FromResult(new RenderOutput(thickness, 0, null, false));
        }

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SkiaUtils.ConvertToSkColor(color),
            StrokeWidth = thickness,
            IsAntialias = true
        };

        float startX = lineContentX;
        float endX = lineContentX + lineContentWidth;
        float lineDrawY = currentY + thickness / 2f;

        canvas.DrawLine(startX, lineDrawY, endX, lineDrawY, paint);

        var renderOutput = new RenderOutput(thickness, lineContentWidth, null, false);
        return Task.FromResult(renderOutput);
    }
}
