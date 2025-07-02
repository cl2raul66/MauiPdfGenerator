using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal interface IElementRenderer
{
    Task<LayoutInfo> MeasureAsync(PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect availableRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry);

    Task RenderAsync(SKCanvas canvas, PdfElement element, ElementRendererFactory rendererFactory, PdfPageData pageDef, SKRect renderRect, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry);
}
