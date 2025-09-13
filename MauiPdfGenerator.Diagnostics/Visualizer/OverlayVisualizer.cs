using MauiPdfGenerator.Diagnostics.Contracts;
using MauiPdfGenerator.Core.Models;
using SkiaSharp;
using MauiPdfGenerator.Core.Implementation.Sk;

namespace MauiPdfGenerator.Diagnostics.Visualizer;

internal class OverlayVisualizer : IPdfDiagnosticVisualizer
{
    public Task DrawOverlayAsync(SKCanvas canvas, PdfRect bounds, PdfDiagnosticEvent diagnosticEvent, PdfGenerationContext context)
    {
        var skBounds = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);

        using var fillPaint = new SKPaint
        {
            Color = SKColors.Red.WithAlpha(20), // 8% alpha
            Style = SKPaintStyle.Fill
        };

        using var strokePaint = new SKPaint
        {
            Color = SKColors.Red,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 1,
            IsAntialias = true
        };

        canvas.DrawRect(skBounds, fillPaint);
        canvas.DrawRect(skBounds, strokePaint);

        var label = $"[{diagnosticEvent.Code}] {diagnosticEvent.Message}";
        using var font = new SKFont(SKTypeface.Default, 8);
        using var labelPaint = new SKPaint(font)
        {
            Color = SKColors.Red,
            IsAntialias = true
        };
        using var labelBgPaint = new SKPaint
        {
            Color = SKColors.White.WithAlpha(200)
        };

        var textBounds = new SKRect();
        labelPaint.MeasureText(label, ref textBounds);

        var labelRect = new SKRect(skBounds.Left, skBounds.Top, skBounds.Left + textBounds.Width + 4, skBounds.Top + textBounds.Height + 4);

        canvas.DrawRect(labelRect, labelBgPaint);
        canvas.DrawText(label, skBounds.Left + 2, skBounds.Top - textBounds.Top + 2, font, labelPaint);

        return Task.CompletedTask;
    }
}
