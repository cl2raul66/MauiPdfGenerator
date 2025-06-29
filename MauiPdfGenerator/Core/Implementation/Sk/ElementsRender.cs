using MauiPdfGenerator.Core.Implementation.Sk.Elements;
using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;
using SkiaSharp;

namespace MauiPdfGenerator.Core.Implementation.Sk;

internal class ElementsRender
{
    private readonly TextRenderer _textRenderer = new();
    private readonly ImageRenderer _imageRenderer = new();
    private readonly HorizontalLineRender _horizontalLineRenderer = new();
    private readonly PdfGridRender _gridRender = new();
    private readonly PdfVerticalStackLayoutRender _vStackRender = new();
    private readonly PdfHorizontalStackLayoutRender _hStackRender = new();

    internal async Task<RenderOutput> Render(SKCanvas canvas, PdfElement element, PdfPageData pageDef, SKRect availableRect, float currentY, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        return element switch
        {
            PdfParagraph para => await _textRenderer.RenderAsync(canvas, para, pageDef, availableRect, currentY, fontRegistry),
            PdfImage img => await _imageRenderer.RenderAsync(canvas, img, pageDef, availableRect, currentY),
            PdfHorizontalLine line => await _horizontalLineRenderer.RenderAsync(canvas, line, availableRect, currentY),
            PdfGrid grid => await _gridRender.RenderAsync(canvas, grid, pageDef, this, availableRect, currentY, layoutState, fontRegistry),
            PdfVerticalStackLayout vsl => await _vStackRender.RenderAsync(canvas, vsl, pageDef, this, availableRect, currentY, layoutState, fontRegistry),
            PdfHorizontalStackLayout hsl => await _hStackRender.RenderAsync(canvas, hsl, pageDef, this, availableRect, currentY, layoutState, fontRegistry),
            _ => throw new NotImplementedException($"Render not implemented for element type {element.GetType().Name}")
        };
    }

    internal async Task<MeasureOutput> Measure(PdfElement element, PdfPageData pageDef, SKRect availableRect, float currentY, Dictionary<PdfElement, object> layoutState, PdfFontRegistryBuilder fontRegistry)
    {
        var contentRect = new SKRect(availableRect.Left, currentY, availableRect.Right, availableRect.Bottom);

        return element switch
        {
            PdfParagraph para => await _textRenderer.MeasureAsync(para, pageDef, contentRect, currentY, fontRegistry),
            PdfImage img => await _imageRenderer.MeasureAsync(img, pageDef, contentRect, currentY),
            PdfHorizontalLine line => await _horizontalLineRenderer.MeasureAsync(line, contentRect),
            PdfGrid grid => await _gridRender.MeasureAsync(grid, pageDef, this, contentRect, layoutState, fontRegistry),
            PdfVerticalStackLayout vsl => await _vStackRender.MeasureAsync(vsl, pageDef, this, contentRect, layoutState, fontRegistry),
            PdfHorizontalStackLayout hsl => await _hStackRender.MeasureAsync(hsl, pageDef, this, contentRect, layoutState, fontRegistry),
            _ => throw new NotImplementedException($"Measure not implemented for element type {element.GetType().Name}")
        };
    }
}
