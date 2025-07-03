using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IElementRenderer
{
    Task<LayoutInfo> MeasureAsync(PdfGenerationContext context, SKRect availableRect);

    Task RenderAsync(SKCanvas canvas, SKRect renderRect, PdfGenerationContext context);
}
