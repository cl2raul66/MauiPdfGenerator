using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Implementation.Builders;
using MauiPdfGenerator.Implementation.Layout.Managers;
using MauiPdfGenerator.Implementation.Layout.Models;
using System.Diagnostics;

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Implementación principal del motor de layout.
/// Coordina las operaciones de medición y posicionamiento.
/// </summary>
internal class LayoutEngine : IMeasureEngine, IArrangeEngine
{
    private readonly Dictionary<object, LayoutResult> _layoutCache = [];
    private readonly VerticalStackLayoutManager _verticalStackLayoutManager;
    private readonly ParagraphManager _paragraphManager;
    // private LayoutContext? _currentContext; // Eliminado - El contexto se pasa como parámetro

    public LayoutEngine()
    {
        _verticalStackLayoutManager = new VerticalStackLayoutManager(this, this); // Pasamos this (el propio engine) como IMeasureEngine e IArrangeEngine
        _paragraphManager = new ParagraphManager();
        // TODO: Inicializar los otros managers (Image, HSL, Grid) cuando existan
    }

    // Eliminados BeginLayout/EndLayout ya que _currentContext se eliminó

    // --- IMeasureEngine Implementation ---
    public PdfSize Measure(object element, LayoutContext context) // Firma CORRECTA (usa LayoutContext)
    {
        // Si el elemento ya está en caché y no necesita remedirse, devolver el tamaño medido
        if (_layoutCache.TryGetValue(element, out var cachedResult) && !NeedsRemeasure(element))
        {
            return cachedResult.MeasuredSize;
        }

        // Realizar la medición específica según el tipo de elemento, pasando el contexto
        var measuredSize = MeasureElement(element, context); // Llama al helper con contexto

        // Almacenar o actualizar el resultado en caché
        var result = LayoutResult.CreateSuccess(element, measuredSize);
        _layoutCache[element] = result;

        return measuredSize;
    }

    // --- IArrangeEngine Implementation ---
    public void Arrange(object element, LayoutContext context) // Firma CORRECTA (usa LayoutContext)
    {
        var finalRect = context.AvailableArea; // El rect final viene del contexto
        LayoutResult? result; // Usar nullable

        // Intentar obtener del caché. Si no existe O necesita remedirse, medir primero.
        if (!_layoutCache.TryGetValue(element, out result) || result == null || NeedsRemeasure(element))
        {
            // Medir para obtener el tamaño y poblar/actualizar caché
            // No necesitamos crear un 'measureContext' especial aquí, el 'context'
            // recibido ya tiene el área disponible correcta para esta fase.
            var measuredSize = Measure(element, context); // Llama a Measure con el contexto actual

            // Volver a intentar obtener del caché después de Measure
            if (!_layoutCache.TryGetValue(element, out result) || result == null)
            {
                // Si Measure falló o no añadió al caché
                throw new InvalidOperationException($"Element of type {element.GetType().Name} could not be measured and cannot be arranged.");
            }
        }

        // Establecer/Actualizar el rectángulo final en el resultado cacheado
        result.SetFinalRect(finalRect); // finalRect viene del 'context' de Arrange

        // Realizar el posicionamiento específico si es necesario, pasando el contexto
        if (NeedsArrange(element))
        {
            ArrangeElement(element, context); // Llama al helper con contexto
        }
    }

    // --- Helpers Internos ---

    private PdfSize MeasureElement(object element, LayoutContext context) // Firma CORRECTA (usa LayoutContext)
    {
        // Llama a los métodos Measure... específicos, pasando el CONTEXTO
        return element switch
        {
            ParagraphBuilder paragraphBuilder => MeasureParagraph(paragraphBuilder, context),
            ImageBuilder imageBuilder => MeasureImage(imageBuilder, context),
            VerticalStackLayoutBuilder vslBuilder => MeasureVerticalStackLayout(vslBuilder, context),
            HorizontalStackLayoutBuilder hslBuilder => MeasureHorizontalStackLayout(hslBuilder, context),
            GridBuilder gridBuilder => MeasureGrid(gridBuilder, context),
            _ => throw new ArgumentException($"MeasureElement: Tipo de elemento no soportado: {element.GetType()}")
        };
    }

    private void ArrangeElement(object element, LayoutContext context) // Firma CORRECTA (usa LayoutContext)
    {
        // Llama a los métodos Arrange... específicos, pasando el CONTEXTO
        switch (element)
        {
            case ParagraphBuilder paragraphBuilder:
                ArrangeParagraph(paragraphBuilder, context);
                break;
            case ImageBuilder imageBuilder:
                ArrangeImage(imageBuilder, context);
                break;
            case VerticalStackLayoutBuilder vslBuilder:
                ArrangeVerticalStackLayout(vslBuilder, context);
                break;
            case HorizontalStackLayoutBuilder hslBuilder:
                ArrangeHorizontalStackLayout(hslBuilder, context);
                break;
            case GridBuilder gridBuilder:
                ArrangeGrid(gridBuilder, context);
                break;
            default:
                throw new ArgumentException($"ArrangeElement: Tipo de elemento no soportado: {element.GetType()}");
        }
    }

    // --- Métodos de Medición Específicos ---
    // Todos deben aceptar LayoutContext y llamar al Measure del Manager correspondiente con el contexto.

    private PdfSize MeasureParagraph(ParagraphBuilder builder, LayoutContext context)
    {
        return _paragraphManager.Measure(builder, context); // Pasa el contexto
    }

    private PdfSize MeasureImage(ImageBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: return _imageManager.Measure(builder, context);
        Debug.WriteLine($"Warning: MeasureImage for {builder.GetType().Name} not implemented.");
        return PdfSize.Zero; // Placeholder temporal
        // throw new NotImplementedException("MeasureImage");
    }

    private PdfSize MeasureVerticalStackLayout(VerticalStackLayoutBuilder builder, LayoutContext context)
    {
        return _verticalStackLayoutManager.Measure(builder, context); // Pasa el contexto
    }

    private PdfSize MeasureHorizontalStackLayout(HorizontalStackLayoutBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: return _hslManager.Measure(builder, context);
        Debug.WriteLine($"Warning: MeasureHorizontalStackLayout for {builder.GetType().Name} not implemented.");
        return PdfSize.Zero; // Placeholder temporal
        // throw new NotImplementedException("MeasureHorizontalStackLayout");
    }

    private PdfSize MeasureGrid(GridBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: return _gridManager.Measure(builder, context);
        Debug.WriteLine($"Warning: MeasureGrid for {builder.GetType().Name} not implemented.");
        return PdfSize.Zero; // Placeholder temporal
        // throw new NotImplementedException("MeasureGrid");
    }

    // --- Métodos de Posicionamiento Específicos ---
    // Todos deben aceptar LayoutContext y llamar al Arrange del Manager correspondiente con el contexto.

    private void ArrangeParagraph(ParagraphBuilder builder, LayoutContext context)
    {
        _paragraphManager.Arrange(builder, context); // Pasa el contexto
    }

    private void ArrangeImage(ImageBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: _imageManager.Arrange(builder, context);
        Debug.WriteLine($"Warning: ArrangeImage for {builder.GetType().Name} not implemented.");
        // throw new NotImplementedException("ArrangeImage");
    }

    private void ArrangeVerticalStackLayout(VerticalStackLayoutBuilder builder, LayoutContext context)
    {
        _verticalStackLayoutManager.Arrange(builder, context); // Pasa el contexto
    }

    private void ArrangeHorizontalStackLayout(HorizontalStackLayoutBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: _hslManager.Arrange(builder, context);
        Debug.WriteLine($"Warning: ArrangeHorizontalStackLayout for {builder.GetType().Name} not implemented.");
        // throw new NotImplementedException("ArrangeHorizontalStackLayout");
    }

    private void ArrangeGrid(GridBuilder builder, LayoutContext context)
    {
        // TODO: Implementar Manager y llamar: _gridManager.Arrange(builder, context);
        Debug.WriteLine($"Warning: ArrangeGrid for {builder.GetType().Name} not implemented.");
        // throw new NotImplementedException("ArrangeGrid");
    }


    // --- Métodos de Invalidadción (Simplificados) ---
    public bool NeedsRemeasure(object element)
    {
        // TODO: Implementar lógica de invalidación real (ej. si propiedades del elemento cambiaron)
        return true; // Por ahora, siempre remedir
    }

    public bool NeedsArrange(object element)
    {
        // TODO: Implementar lógica de invalidación real (ej. si tamaño medido cambió o posición padre cambió)
        return true; // Por ahora, siempre reposicionar
    }


    // --- Método Arrange(object, PdfRectangle) ELIMINADO ---
    // Este método duplicado que aceptaba PdfRectangle causaba ambigüedad y errores.
    // public void Arrange(object element, PdfRectangle finalRect)
    // {
    //     throw new NotImplementedException(); // ¡Eliminar completamente!
    // }

}
