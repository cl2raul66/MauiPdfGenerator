// Ignore Spelling: Skia

using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Diagnostics.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Utils;

internal class SkiaDiagnosticCanvasAdapter : IDiagnosticCanvas
{
    private readonly SKCanvas _canvas;

    public SkiaDiagnosticCanvasAdapter(SKCanvas canvas)
    {
        _canvas = canvas;
    }

    public void DrawLabel(string text, PointF position, Color color, float fontSize)
    {
        using var typeface = SKTypeface.Default;
        using var font = new SKFont(typeface, fontSize);
        using var paint = new SKPaint
        {
            Color = SkiaUtils.ConvertToSkColor(color),
            IsAntialias = true
        };

        _canvas.DrawText(text, position.X, position.Y + fontSize, font, paint);
    }

    public void DrawRectangle(DiagnosticRect bounds, Color color, float thickness, bool isDashed)
    {
        using var paint = new SKPaint
        {
            Color = SkiaUtils.ConvertToSkColor(color),
            StrokeWidth = thickness,
            Style = SKPaintStyle.Stroke,
            IsAntialias = true
        };

        if (isDashed)
        {
            paint.PathEffect = SKPathEffect.CreateDash([5, 5], 0);
        }

        var rect = new SKRect(bounds.Left, bounds.Top, bounds.Right, bounds.Bottom);
        _canvas.DrawRect(rect, paint);
    }
}
