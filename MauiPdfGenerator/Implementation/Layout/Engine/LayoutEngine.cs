using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Implementation.Builders;
using MauiPdfGenerator.Implementation.Layout.Managers;
using MauiPdfGenerator.Implementation.Layout.Models;

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Implementación principal del motor de layout.
/// Coordina las operaciones de medición y posicionamiento.
/// </summary>
internal class LayoutEngine : IMeasureEngine, IArrangeEngine
{
    private readonly Dictionary<object, LayoutResult> _layoutCache = new();
    private readonly VerticalStackLayoutManager _verticalStackLayoutManager;
    private readonly ParagraphManager _paragraphManager;
    private LayoutContext? _currentContext;

    public LayoutEngine()
    {
        _verticalStackLayoutManager = new VerticalStackLayoutManager(this, this);
        _paragraphManager = new ParagraphManager();
    }

    public void BeginLayout(LayoutContext context)
    {
        _currentContext = context;
    }

    public void EndLayout()
    {
        _currentContext = null;
    }

    public PdfSize Measure(object element, PdfSize availableSize)
    {
        // Si el elemento ya está en caché y no necesita remedirse, devolver el tamaño medido
        if (_layoutCache.TryGetValue(element, out var cachedResult) && !NeedsRemeasure(element))
        {
            return cachedResult.MeasuredSize;
        }

        // Realizar la medición específica según el tipo de elemento
        var measuredSize = MeasureElement(element, availableSize);
        
        // Almacenar o actualizar el resultado en caché
        var result = LayoutResult.CreateSuccess(element, measuredSize);
        _layoutCache[element] = result;

        return measuredSize;
    }

    public void Arrange(object element, PdfRectangle finalRect)
    {
        if (!_layoutCache.TryGetValue(element, out var result))
        {
            // Si el elemento no ha sido medido, medirlo primero
            var measuredSize = Measure(element, new PdfSize(finalRect.Width, finalRect.Height));
            result = _layoutCache[element];
        }

        // Actualizar el rectángulo final
        result.SetFinalRect(finalRect);

        // Realizar el posicionamiento específico según el tipo de elemento
        ArrangeElement(element, finalRect);
    }

    public bool NeedsRemeasure(object element)
    {
        // Por defecto, asumimos que los elementos necesitan remedirse
        // Esta lógica puede mejorarse con un sistema de invalidación más sofisticado
        return true;
    }

    public bool NeedsArrange(object element)
    {
        // Por defecto, asumimos que los elementos necesitan reposicionarse
        // Esta lógica puede mejorarse con un sistema de invalidación más sofisticado
        return true;
    }

    private PdfSize MeasureElement(object element, PdfSize availableSize)
    {
        return element switch
        {
            // Implementar la lógica de medición específica para cada tipo de elemento
            // Por ejemplo:
            ParagraphBuilder paragraphBuilder => MeasureParagraph(paragraphBuilder, availableSize),
            ImageBuilder imageBuilder => MeasureImage(imageBuilder, availableSize),
            VerticalStackLayoutBuilder vslBuilder => MeasureVerticalStackLayout(vslBuilder, availableSize),
            HorizontalStackLayoutBuilder hslBuilder => MeasureHorizontalStackLayout(hslBuilder, availableSize),
            GridBuilder gridBuilder => MeasureGrid(gridBuilder, availableSize),
            _ => throw new ArgumentException($"Tipo de elemento no soportado: {element.GetType()}")
        };
    }

    private void ArrangeElement(object element, PdfRectangle finalRect)
    {
        switch (element)
        {
            // Implementar la lógica de posicionamiento específica para cada tipo de elemento
            case ParagraphBuilder paragraphBuilder:
                ArrangeParagraph(paragraphBuilder, finalRect);
                break;
            case ImageBuilder imageBuilder:
                ArrangeImage(imageBuilder, finalRect);
                break;
            case VerticalStackLayoutBuilder vslBuilder:
                ArrangeVerticalStackLayout(vslBuilder, finalRect);
                break;
            case HorizontalStackLayoutBuilder hslBuilder:
                ArrangeHorizontalStackLayout(hslBuilder, finalRect);
                break;
            case GridBuilder gridBuilder:
                ArrangeGrid(gridBuilder, finalRect);
                break;
            default:
                throw new ArgumentException($"Tipo de elemento no soportado: {element.GetType()}");
        }
    }

    private PdfSize MeasureParagraph(ParagraphBuilder builder, PdfSize availableSize)
    {
        if (_currentContext == null)
            throw new InvalidOperationException("Layout operation must be performed within a layout context.");

        var childContext = _currentContext.CreateChildContext(
            new PdfRectangle(0, 0, availableSize.Width, availableSize.Height)
        );

        return _paragraphManager.Measure(builder, childContext);
    }

    private void ArrangeParagraph(ParagraphBuilder builder, PdfRectangle finalRect)
    {
        if (_currentContext == null)
            throw new InvalidOperationException("Layout operation must be performed within a layout context.");

        var childContext = _currentContext.CreateChildContext(finalRect);
        _paragraphManager.Arrange(builder, childContext);
    }

    private PdfSize MeasureImage(ImageBuilder builder, PdfSize availableSize)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }

    private void ArrangeImage(ImageBuilder builder, PdfRectangle finalRect)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }

    private PdfSize MeasureVerticalStackLayout(VerticalStackLayoutBuilder builder, PdfSize availableSize)
    {
        if (_currentContext == null)
            throw new InvalidOperationException("Layout operation must be performed within a layout context.");

        var childContext = _currentContext.CreateChildContext(
            new PdfRectangle(0, 0, availableSize.Width, availableSize.Height)
        );

        return _verticalStackLayoutManager.Measure(builder, childContext);
    }

    private void ArrangeVerticalStackLayout(VerticalStackLayoutBuilder builder, PdfRectangle finalRect)
    {
        if (_currentContext == null)
            throw new InvalidOperationException("Layout operation must be performed within a layout context.");

        var childContext = _currentContext.CreateChildContext(finalRect);
        _verticalStackLayoutManager.Arrange(builder, childContext);
    }

    private PdfSize MeasureHorizontalStackLayout(HorizontalStackLayoutBuilder builder, PdfSize availableSize)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }

    private void ArrangeHorizontalStackLayout(HorizontalStackLayoutBuilder builder, PdfRectangle finalRect)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }

    private PdfSize MeasureGrid(GridBuilder builder, PdfSize availableSize)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }

    private void ArrangeGrid(GridBuilder builder, PdfRectangle finalRect)
    {
        // Implementación pendiente
        throw new NotImplementedException();
    }
}