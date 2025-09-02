using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Common.Models.Layouts;

namespace MauiPdfGenerator.Core.Implementation.Sk.Elements;

internal class ElementRendererFactory
{
    private readonly Dictionary<Type, IElementRenderer> _renderers;

    public ElementRendererFactory()
    {
        _renderers = new Dictionary<Type, IElementRenderer>
        {
            { typeof(PdfParagraphData), new TextRenderer() },
            { typeof(PdfImageData), new ImageRenderer() },
            { typeof(PdfHorizontalLineData), new HorizontalLineRender() },
            { typeof(PdfVerticalStackLayoutData), new PdfVerticalStackLayoutRender() },
            { typeof(PdfHorizontalStackLayoutData), new PdfHorizontalStackLayoutRender() }
        };
    }

    public IElementRenderer GetRenderer(object element)
    {
        ArgumentNullException.ThrowIfNull(element);
        if (_renderers.TryGetValue(element.GetType(), out var renderer))
        {
            return renderer;
        }
        throw new NotImplementedException($"No renderer registered for element type {element.GetType().Name}");
    }
}
