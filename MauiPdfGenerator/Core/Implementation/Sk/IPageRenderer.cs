using MauiPdfGenerator.Core.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IPageRenderer
{
    Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfGenerationContext context);

    Task RenderPageBlockAsync(
        SKCanvas canvas,
        IReadOnlyList<LayoutInfo> pageBlock,
        PdfGenerationContext context);
}
