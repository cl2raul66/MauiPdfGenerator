using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender
{
    internal RenderOutput Render(SKCanvas canvas, PdfHorizontalLine line, SKRect contentRect, float currentY)
    {
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineContentX = contentRect.Left + (float)line.GetMargin.Left;
        float lineContentWidth = contentRect.Width - (float)line.GetMargin.Left - (float)line.GetMargin.Right;

        if (lineContentWidth <= 0) return new RenderOutput(thickness, 0, null, false);

        float availableHeightForLineContent = contentRect.Bottom - currentY - (float)line.GetMargin.Bottom;

        if (thickness > availableHeightForLineContent)
        {
            return new RenderOutput(thickness, lineContentWidth, null, true);
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

        if (lineDrawY - thickness / 2f < contentRect.Top - 0.01f || lineDrawY + thickness / 2f > contentRect.Bottom + 0.01f)
        {
            return new RenderOutput(thickness, lineContentWidth, null, true);
        }

        canvas.DrawLine(startX, lineDrawY, endX, lineDrawY, paint);
        return new RenderOutput(thickness, lineContentWidth, null, false);
    }
}
