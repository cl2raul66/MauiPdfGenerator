using MauiPdfGenerator.Core.Implementation.Sk.Layouts;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Common.Models.Layouts;

namespace MauiPdfGenerator.Core.Implementation.Sk.Views;

internal class ElementRendererFactory : IElementRendererFactory
{
    private readonly Dictionary<Type, IElementRenderer> _renderers;

    public ElementRendererFactory()
    {
        _renderers = new Dictionary<Type, IElementRenderer>
        {
            { typeof(PdfParagraphData), new TextRenderer() },
            { typeof(PdfImageData), new ImageRenderer() },
            { typeof(PdfHorizontalLineData), new HorizontalLineRenderer() },
            { typeof(PdfVerticalStackLayoutData), new VerticalStackLayoutRenderer() },
            { typeof(PdfHorizontalStackLayoutData), new HorizontalStackLayoutRenderer() },
            { typeof(PdfGridData), new GridRenderer() } 
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
