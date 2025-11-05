using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IPageRenderer
{
    Task<List<IReadOnlyList<PdfLayoutInfo>>> LayoutAsync(PdfGenerationContext context);

    Task RenderPageBlockAsync(
        SKCanvas canvas,
        IReadOnlyList<PdfLayoutInfo> pageBlock,
        PdfGenerationContext context);
}
