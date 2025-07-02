using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Fluent.Models;
using MauiPdfGenerator.Fluent.Models.Elements;
using MauiPdfGenerator.Fluent.Models.Layouts;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ElementRendererFactory
{
    private readonly Dictionary<Type, IElementRenderer> _renderers;

    public ElementRendererFactory()
    {
        _renderers = new Dictionary<Type, IElementRenderer>
        {
            { typeof(PdfParagraph), new TextRenderer() },
            { typeof(PdfImage), new ImageRenderer() },
            { typeof(PdfHorizontalLine), new HorizontalLineRender() },
            { typeof(PdfGrid), new PdfGridRender() },
            { typeof(PdfVerticalStackLayout), new PdfVerticalStackLayoutRender() },
            { typeof(PdfHorizontalStackLayout), new PdfHorizontalStackLayoutRender() }
        };
    }

    public IElementRenderer GetRenderer(PdfElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (_renderers.TryGetValue(element.GetType(), out var renderer))
        {
            return renderer;
        }
        throw new NotImplementedException($"No renderer registered for element type {element.GetType().Name}");
    }
}
