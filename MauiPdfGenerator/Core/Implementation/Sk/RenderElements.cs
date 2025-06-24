using MauiPdfGenerator.Core.Implementation.Sk.Elements;
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
    private readonly LayoutRenderer _layoutRenderer = new();
    private readonly HorizontalLineRender _horizontalLineRenderer = new();
    private PdfFontRegistryBuilder? _currentFontRegistry;

    internal async Task<RenderOutput> Render(SKCanvas canvas, PdfElement element, PdfPageData pageDef, SKRect availableRect, float currentY, PdfFontRegistryBuilder fontRegistry)
    {
        _currentFontRegistry = fontRegistry;
        return await RenderToOutput(canvas,element, pageDef, availableRect, currentY);
    }

    internal async Task<RenderOutput> RenderToOutput(SKCanvas canvas, PdfElement element, PdfPageData pageDef, SKRect availableRect, float currentY)
    {
        // Debug: Log tipo, posición y detalles del elemento
        string tipo = element.GetType().Name;
        string texto = (element is PdfParagraph p) ? p.Text : (element is PdfImage ? "[PdfImage]" : "");
        System.Diagnostics.Debug.WriteLine($"[RenderToOutput] Dibujando elemento tipo {tipo} en [{availableRect.Left},{availableRect.Top}] tamaño [{availableRect.Width}x{availableRect.Height}] texto: {texto}");

        return element switch
        {
            PdfParagraph para => await _textRenderer.RenderAsync(canvas, para, pageDef, availableRect, currentY, _currentFontRegistry),
            PdfImage img => await _imageRenderer.RenderAsync(canvas, img, pageDef, availableRect, currentY),
            PdfHorizontalLine line => _horizontalLineRenderer.Render(canvas, line, availableRect, currentY),
            PdfGrid grid => await _layoutRenderer.RenderGridAsLayoutAsync(canvas, grid, pageDef, availableRect, currentY, RenderToOutput),
            PdfVerticalStackLayout vsl => await _layoutRenderer.RenderVerticalStackLayoutAsync(canvas, vsl, pageDef, availableRect, currentY, RenderToOutput),
            PdfHorizontalStackLayout hsl => await _layoutRenderer.RenderHorizontalStackLayoutAsync(canvas, hsl, pageDef, availableRect, currentY, RenderToOutput),
            _ => new RenderOutput(0, 0, null, false),
        };
    }

    // Nuevo método para renderizar la página usando la estrategia automática
    internal async Task<RenderOutput> RenderPageAuto(SKCanvas canvas, PdfPageData pageDef, SKRect contentRect, PdfFontRegistryBuilder fontRegistry)
    {
        return await _layoutRenderer.RenderPdfContentPageAutoAsync(canvas, pageDef, contentRect, Render, fontRegistry);
    }
}
