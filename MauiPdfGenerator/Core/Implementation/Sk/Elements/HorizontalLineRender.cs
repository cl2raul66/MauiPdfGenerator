using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class HorizontalLineRender : IElementRenderer
{
    public Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var line = (PdfHorizontalLine)element;
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        float width = availableRect.Width;
        float height = thickness + (float)line.GetMargin.VerticalThickness;

        var layoutInfo = new LayoutInfo(element, width, height);
        return Task.FromResult(layoutInfo);
    }

    public Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var line = (PdfHorizontalLine)element;
        float thickness = line.CurrentThickness > 0 ? line.CurrentThickness : PdfHorizontalLine.DefaultThickness;
        Color color = line.CurrentColor ?? PdfHorizontalLine.DefaultColor;
        if (thickness <= 0) thickness = PdfHorizontalLine.DefaultThickness;

        float lineY = renderRect.Top + (float)line.GetMargin.Top + thickness / 2f;
        float startX = renderRect.Left + (float)line.GetMargin.Left;
        float endX = renderRect.Right - (float)line.GetMargin.Right;

        if (endX - startX <= 0)
        {
            return Task.CompletedTask;
        }

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
