using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IElementRenderer
{
    Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect);
    Task<LayoutInfo> ArrangeAsync(PdfRect finalRect, PdfGenerationContext context);
    Task RenderAsync(SKCanvas canvas, PdfGenerationContext context);
}
