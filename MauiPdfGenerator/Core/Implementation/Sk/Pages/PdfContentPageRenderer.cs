using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfContentPageRenderer : IPageRenderer
{
    private readonly PageLayoutEngine _layoutEngine = new();
    private readonly ElementRendererFactory _rendererFactory = new();

    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfPageData pageDef, PdfFontRegistryBuilder fontRegistry, Dictionary<PdfElement, object> layoutState)
    {
        return await _layoutEngine.LayoutAsync(pageDef.Elements, pageDef, layoutState, fontRegistry);
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, PdfPageData pageDef, IReadOnlyList<LayoutInfo> pageBlock, PdfFontRegistryBuilder fontRegistry, Dictionary<PdfElement, object> layoutState)
    {
        canvas.Clear(pageDef.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageDef.BackgroundColor) : SKColors.White);

        SKSize pageSize = SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        var pageMargins = pageDef.Margins;
        var contentRect = new SKRect(
            (float)pageMargins.Left, (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right, pageSize.Height - (float)pageMargins.Bottom
        );

        canvas.Save();
        canvas.ClipRect(contentRect);

        float currentY = contentRect.Top;
        for (int i = 0; i < pageBlock.Count; i++)
        {
            var layoutInfo = pageBlock[i];
            var element = layoutInfo.Element;
            var renderer = _rendererFactory.GetRenderer(element);

            float offsetX = element.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentRect.Width - layoutInfo.Width) / 2f,
                LayoutAlignment.End => contentRect.Width - layoutInfo.Width,
                _ => 0f
            };

            var renderRect = SKRect.Create(contentRect.Left + offsetX, currentY, layoutInfo.Width, layoutInfo.Height);
            await renderer.RenderAsync(canvas, element, _rendererFactory, pageDef, renderRect, layoutState, fontRegistry);

            currentY += layoutInfo.Height;
            if (i < pageBlock.Count - 1)
            {
                currentY += pageDef.PageDefaultSpacing;
            }
        }

        canvas.Restore();
    }
}
