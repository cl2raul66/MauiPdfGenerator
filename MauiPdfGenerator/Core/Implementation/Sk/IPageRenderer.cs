using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IPageRenderer
{
    Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(
       PdfPageData pageData,
       PdfFontRegistryBuilder fontRegistry,
       Dictionary<PdfElement, object> layoutState);

    Task RenderPageBlockAsync(
        SKCanvas canvas,
        PdfPageData pageData,
        IReadOnlyList<LayoutInfo> pageBlock,
        PdfFontRegistryBuilder fontRegistry,
        Dictionary<PdfElement, object> layoutState);
}
