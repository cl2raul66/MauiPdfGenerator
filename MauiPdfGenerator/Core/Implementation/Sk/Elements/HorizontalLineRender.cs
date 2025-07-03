using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender : IElementRenderer
{
    public Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect)
    {
        if (context.Element is not PdfHorizontalLine line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLine)} or is null.");

        float thickness = line.CurrentThickness;

        float heightOfBox = thickness;
        float totalHeightWithMargin = heightOfBox + (float)line.GetMargin.VerticalThickness;

        float widthOfBox = availableRect.Width - (float)line.GetMargin.HorizontalThickness;
        float totalWidthWithMargin = widthOfBox + (float)line.GetMargin.HorizontalThickness;

        var layoutInfo = new LayoutInfo(line, totalWidthWithMargin, totalHeightWithMargin);
        return Task.FromResult(layoutInfo);
    }

    public Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context)
    {
        if (context.Element is not PdfHorizontalLine line)
            throw new InvalidOperationException($"Element in context is not a {nameof(PdfHorizontalLine)} or is null.");

        float thickness = line.CurrentThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;

        var elementRect = new SKRect(
            renderRect.Left + (float)line.GetMargin.Left,
            renderRect.Top + (float)line.GetMargin.Top,
            renderRect.Right - (float)line.GetMargin.Right,
            renderRect.Bottom - (float)line.GetMargin.Bottom
        );

        if (elementRect.Width <= 0)
        {
            return Task.CompletedTask;
        }

        float lineY = elementRect.MidY;
        float startX = elementRect.Left;
        float endX = elementRect.Right;

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SkiaUtils.ConvertToSkColor(color),
            StrokeWidth = thickness,
            IsAntialias = true
        };

        canvas.DrawLine(startX, lineY, endX, lineY, paint);
        return Task.CompletedTask;
    }
}
