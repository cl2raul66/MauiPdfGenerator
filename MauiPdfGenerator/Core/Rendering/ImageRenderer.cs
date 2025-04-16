// --- START OF FILE MauiPdfGenerator/Core/Rendering/ImageRenderer.cs ---
using SkiaSharp;
using MauiPdfGenerator.Common.Elements;
using MauiPdfGenerator.Core.Utils; // For SkiaValueConverter
using System;

namespace MauiPdfGenerator.Core.Rendering;

/// <summary>
/// Handles rendering of ImageElementModel onto an SKCanvas.
/// Assumes element coordinates and sizes are already in points.
/// </summary>
internal static class ImageRenderer
{
    // No longer needs UnitConverter
    public static void Render(SKCanvas canvas, ImageElementModel element)
    {
        if (element.ImageData == null || element.ImageData.Length == 0) return;

        using var skImage = SKImage.FromEncodedData(element.ImageData);
        if (skImage == null)
        {
            Console.WriteLine("Warning: Failed to decode image data.");
            return;
        }

        // TargetRect is already in points
        var targetRectInPoints = SkiaValueConverter.ToSKRect(element.TargetRect);

        if (targetRectInPoints.Width <= 0 || targetRectInPoints.Height <= 0)
        {
            Console.WriteLine("Warning: Image target rectangle has zero or negative dimensions.");
            return;
        }

        using var paint = new SKPaint { FilterQuality = SKFilterQuality.High };
        canvas.DrawImage(skImage, targetRectInPoints, paint);
    }
}
// --- END OF FILE MauiPdfGenerator/Core/Rendering/ImageRenderer.cs ---
