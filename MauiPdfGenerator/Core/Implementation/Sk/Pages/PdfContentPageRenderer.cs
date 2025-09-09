using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Common.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfContentPageRenderer : IPageRenderer
{
    private readonly PageLayoutEngine _layoutEngine = new();

    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        return await _layoutEngine.LayoutAsync(context);
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, IReadOnlyList<LayoutInfo> arrangedPageBlock, PdfGenerationContext context)
    {
        var pageDef = context.PageData;
        canvas.Clear(pageDef.BackgroundColor is not null ? SkiaUtils.ConvertToSkColor(pageDef.BackgroundColor) : SKColors.White);

        SKSize pageSize = SkiaUtils.GetSkPageSize(pageDef.Size, pageDef.Orientation);
        var pageMargins = pageDef.Padding;
        var contentRect = new SKRect(
            (float)pageMargins.Left, (float)pageMargins.Top,
            pageSize.Width - (float)pageMargins.Right, pageSize.Height - (float)pageMargins.Bottom
        );

        canvas.Save();
        canvas.ClipRect(contentRect);
        foreach (var layoutInfo in arrangedPageBlock)
        {
            var element = (PdfElementData)layoutInfo.Element;
            var renderer = context.RendererFactory.GetRenderer(element);
            var elementContext = context with { Element = element };

            await renderer.RenderAsync(canvas, elementContext);
        }

        canvas.Restore();
    }
}
