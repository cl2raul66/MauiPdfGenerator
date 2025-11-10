using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IElementRenderer
{
    Task<PdfLayoutInfo> MeasureAsync(PdfGenerationContext context, SKSize availableRect);
    Task<PdfLayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context);
    Task RenderAsync(SKCanvas canvas, PdfGenerationContext context);
    Task RenderOverflowAsync(SKCanvas canvas, PdfRect bounds, PdfGenerationContext context);
}
