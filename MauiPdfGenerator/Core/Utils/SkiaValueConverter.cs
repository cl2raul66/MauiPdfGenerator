// --- START OF FILE MauiPdfGenerator/Core/Utils/SkiaValueConverter.cs ---
using SkiaSharp;
using MauiPdfGenerator.Common.Primitives; // For PdfColor, PdfPoint, etc.

namespace MauiPdfGenerator.Core.Utils;

/// <summary>
/// Static utility class for converting primitive types from the Common namespace
/// (which are now assumed to be primarily in points) to their SkiaSharp equivalents.
/// </summary>
internal static class SkiaValueConverter
{
    public static SKColor ToSKColor(PdfColor color)
    {
        return new SKColor(color.R, color.G, color.B, color.A);
    }

    /// <summary> Converts Common.PdfPoint (assumed points) to SkiaSharp.SKPoint. </summary>
    public static SKPoint ToSKPoint(PdfPoint pointInPoints)
    {
        return new SKPoint(pointInPoints.X, pointInPoints.Y);
    }

    /// <summary> Converts Common.PdfSize (assumed points, for page size) to SkiaSharp.SKSize. </summary>
    public static SKSize ToSKPageSize(PdfSize pageSizeInPoints)
    {
        return new SKSize(pageSizeInPoints.Width, pageSizeInPoints.Height);
    }

    /// <summary> Converts Common.PdfSize (assumed points, for element size) to SkiaSharp.SKSize. </summary>
    public static SKSize ToSKElementSize(PdfSize sizeInPoints)
    {
        return new SKSize(sizeInPoints.Width, sizeInPoints.Height);
    }

    /// <summary> Converts Common.PdfRect (assumed points) to SkiaSharp.SKRect. </summary>
    public static SKRect ToSKRect(PdfRect rectInPoints)
    {
        return SKRect.Create(rectInPoints.X, rectInPoints.Y, rectInPoints.Width, rectInPoints.Height);
    }

    /// <summary> Converts Common.PdfHorizontalAlignment to SkiaSharp.SKTextAlign. </summary>
    public static SKTextAlign ToSKTextAlign(PdfHorizontalAlignment alignment)
    {
        return alignment switch
        {
            PdfHorizontalAlignment.Left => SKTextAlign.Left,
            PdfHorizontalAlignment.Center => SKTextAlign.Center,
            PdfHorizontalAlignment.Right => SKTextAlign.Right,
            _ => SKTextAlign.Left,
        };
    }
}
// --- END OF FILE MauiPdfGenerator/Core/Utils/SkiaValueConverter.cs ---
