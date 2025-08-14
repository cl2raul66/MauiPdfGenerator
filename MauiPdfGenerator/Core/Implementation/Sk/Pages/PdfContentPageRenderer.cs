using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Models;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk.Pages;

internal class PdfContentPageRenderer : IPageRenderer
{
    private readonly PageLayoutEngine _layoutEngine = new();

    public async Task<List<IReadOnlyList<LayoutInfo>>> LayoutAsync(PdfGenerationContext context)
    {
        return await _layoutEngine.LayoutAsync(context);
    }

    public async Task RenderPageBlockAsync(SKCanvas canvas, IReadOnlyList<LayoutInfo> pageBlock, PdfGenerationContext context)
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

        float currentY = contentRect.Top;
        for (int i = 0; i < pageBlock.Count; i++)
        {
            var layoutInfo = pageBlock[i];
            var element = (PdfElement)layoutInfo.Element;
            var renderer = context.RendererFactory.GetRenderer(element);

            float offsetX = element.GetHorizontalOptions switch
            {
                LayoutAlignment.Center => (contentRect.Width - layoutInfo.Width) / 2f,
                LayoutAlignment.End => contentRect.Width - layoutInfo.Width,
                _ => 0f
            };

            var renderRect = SKRect.Create(contentRect.Left + offsetX, currentY, layoutInfo.Width, layoutInfo.Height);
            var elementContext = context with { Element = element };
            await renderer.RenderAsync(canvas, renderRect, elementContext);

            currentY += layoutInfo.Height;
            if (i < pageBlock.Count - 1)
            {
                currentY += pageDef.PageDefaultSpacing;
            }
        }

        canvas.Restore();
    }
}
