// --- START OF FILE MauiPdfGenerator/Core/Rendering/ShapeRenderer.cs ---
using SkiaSharp;
using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Core.Utils; // For SkiaValueConverter

namespace MauiPdfGenerator.Core.Rendering;

/// <summary>
/// Handles rendering of basic shape elements (Lines, Rectangles, etc.) onto an SKCanvas.
/// Assumes element coordinates and sizes are already in points.
/// Assumes thickness values are also provided in points.
/// </summary>
internal static class ShapeRenderer
{
    // No longer needs UnitConverter
    public static void RenderLine(SKCanvas canvas, LineElementModel element)
    {
        // Thickness assumed points
        if (element.Thickness <= 0) return;

        // StartPoint and EndPoint are already in points
        var startPoint = SkiaValueConverter.ToSKPoint(element.StartPoint);
        var endPoint = SkiaValueConverter.ToSKPoint(element.EndPoint);
        var color = SkiaValueConverter.ToSKColor(element.Color);
        float thicknessInPoints = element.Thickness; // Assumed points

        using var paint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = color,
            StrokeWidth = thicknessInPoints,
            IsAntialias = true
        };

        canvas.DrawLine(startPoint, endPoint, paint);
    }

    // No longer needs UnitConverter
    public static void RenderRectangle(SKCanvas canvas, RectangleElementModel element)
    {
        // Bounds combines Position and Size, which are already in points
        var rectInPoints = SkiaValueConverter.ToSKRect(element.Bounds);

        // Fill
        if (element.FillColor.A > 0)
        {
            using var fillPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = SkiaValueConverter.ToSKColor(element.FillColor),
                IsAntialias = true
            };
            canvas.DrawRect(rectInPoints, fillPaint);
        }

        // Stroke (Thickness assumed points)
        if (element.StrokeThickness > 0 && element.StrokeColor.A > 0)
        {
            float strokeThicknessInPoints = element.StrokeThickness; // Assumed points
            using var strokePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SkiaValueConverter.ToSKColor(element.StrokeColor),
                StrokeWidth = strokeThicknessInPoints,
                IsAntialias = true
            };
            canvas.DrawRect(rectInPoints, strokePaint);
        }
    }
}
// --- END OF FILE MauiPdfGenerator/Core/Rendering/ShapeRenderer.cs ---
