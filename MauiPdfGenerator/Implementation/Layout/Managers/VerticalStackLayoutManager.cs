using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Implementation.Builders;
using MauiPdfGenerator.Implementation.Layout.Engine;
using MauiPdfGenerator.Implementation.Layout.Models;
using System.Diagnostics;

namespace MauiPdfGenerator.Implementation.Layout.Managers;

/// <summary>
/// Gestiona el layout de elementos apilados verticalmente.
/// </summary>
internal class VerticalStackLayoutManager
{
    private readonly IMeasureEngine _measureEngine;
    private readonly IArrangeEngine _arrangeEngine;

    public VerticalStackLayoutManager(IMeasureEngine measureEngine, IArrangeEngine arrangeEngine)
    {
        _measureEngine = measureEngine ?? throw new ArgumentNullException(nameof(measureEngine));
        _arrangeEngine = arrangeEngine ?? throw new ArgumentNullException(nameof(arrangeEngine));
    }

    public PdfSize Measure(VerticalStackLayoutBuilder builder, LayoutContext context)
    {
        Debug.WriteLine($"context.AvailableArea: {context.AvailableArea}");
        var padding = builder.ConfiguredPadding;
        var spacing = builder.ConfiguredSpacing;
        var children = builder.ConfiguredChildren;

        var availableWidth = context.AvailableArea.Width - padding.Left - padding.Right;
        var availableHeight = context.AvailableArea.Height - padding.Top - padding.Bottom; // Usar altura disponible también
        if (availableWidth < 0) availableWidth = 0;
        if (availableHeight < 0) availableHeight = 0;

        double totalHeight = padding.Top + padding.Bottom;
        double maxWidth = 0;

        // Crear un contexto base para medir los hijos
        // Usamos un área inicial en 0,0 con el tamaño disponible calculado
        // El ContentStream y Resources se heredan del contexto padre
        var childMeasureContextBase = context.CreateChildContext(
            new PdfRectangle(0, 0, availableWidth, availableHeight)
        // Podríamos pasar constraints específicos si el VSL los impusiera
        );


        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];

            // --- CORRECCIÓN ---
            // Medir usando el contexto hijo. El contexto ya contiene el área disponible.
            var childSize = _measureEngine.Measure(child, childMeasureContextBase);
            // --- FIN CORRECCIÓN ---
            Debug.WriteLine($"childSize: {childSize}");

            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;

            if (i < children.Count - 1) { totalHeight += spacing; }
        }

        maxWidth += padding.Left + padding.Right;

        if (builder.ConfiguredWidth.HasValue) { maxWidth = builder.ConfiguredWidth.Value; }
        // Limitar al ancho disponible del contexto padre si no hay ancho explícito? Depende del diseño deseado.
        // maxWidth = Math.Min(maxWidth, context.AvailableArea.Width);

        if (builder.ConfiguredHeight.HasValue) { totalHeight = builder.ConfiguredHeight.Value; }
        // Limitar al alto disponible del contexto padre si no hay alto explícito?
        // totalHeight = Math.Min(totalHeight, context.AvailableArea.Height);


        // Asegurar dimensiones no negativas
        if (maxWidth < 0) maxWidth = 0;
        if (totalHeight < 0) totalHeight = 0;

        var pdfSize = new PdfSize(maxWidth, totalHeight);
        Debug.WriteLine($"PdfSize: {pdfSize}");
        return pdfSize;
    }

    public void Arrange(VerticalStackLayoutBuilder builder, LayoutContext context)
    {
        Debug.WriteLine($"context.AvailableArea: {context.AvailableArea}");
        var padding = builder.ConfiguredPadding;
        var spacing = builder.ConfiguredSpacing;
        var children = builder.ConfiguredChildren;
        var availableArea = context.AvailableArea; // Coordenadas PDF (LL)

        double currentY = availableArea.Y + availableArea.Height - padding.Top;
        var childLeft = availableArea.X + padding.Left;
        var availableChildWidth = availableArea.Width - padding.Left - padding.Right;
        if (availableChildWidth < 0) availableChildWidth = 0;

        // Crear un contexto base para *re-medir* los hijos en Arrange si fuera necesario
        // (Aunque el resultado medido suele venir del caché poblado en la fase Measure)
        // Usamos altura infinita aquí porque en Arrange no limitamos la altura individualmente, solo el ancho.
        var childMeasureContextBaseForArrange = context.CreateChildContext(
           new PdfRectangle(0, 0, availableChildWidth, double.PositiveInfinity)
       );

        foreach (var child in children)
        {            
            // --- CORRECCIÓN ---
            // Volver a medir (o obtener del caché) usando el contexto apropiado
            var childMeasuredSize = _measureEngine.Measure(child, childMeasureContextBaseForArrange);
            // --- FIN CORRECCIÓN ---
            Debug.WriteLine($"childMeasuredSize: {childMeasuredSize}");

            double childX = childLeft;
            if (childMeasuredSize.Width < availableChildWidth)
            {
                var horizontalAlignment = GetChildHorizontalAlignment(child, builder.ConfiguredHorizontalOptions);
                if (horizontalAlignment == PdfHorizontalAlignment.Center)
                { childX = childLeft + (availableChildWidth - childMeasuredSize.Width) / 2; }
                else if (horizontalAlignment == PdfHorizontalAlignment.End)
                { childX = childLeft + availableChildWidth - childMeasuredSize.Width; }
            }

            double childBottomY = currentY - childMeasuredSize.Height;

            var childFinalRect = new PdfRectangle(childX, childBottomY, childMeasuredSize.Width, childMeasuredSize.Height);

            // Crear el contexto para la operación Arrange del hijo
            var childArrangeContext = context.CreateChildContext(childFinalRect);

            Debug.WriteLine($"child.GetType: {child.GetType().Name}");
            Debug.WriteLine($"childFinalRect: {childFinalRect}");

            Debug.WriteLine($"---> VSLManager.Arrange: Calling Arrange for child {child.GetType().Name} in Rect {childFinalRect}");

            // Posicionar usando el contexto de Arrange
            _arrangeEngine.Arrange(child, childArrangeContext);

            currentY = childBottomY - spacing;
        }
    }

    // Helper (podría ir en una clase de utilidad de Layout)
    private PdfHorizontalAlignment GetChildHorizontalAlignment(object childBuilder, PdfHorizontalAlignment defaultAlignment)
    {
        if (childBuilder is IInternalViewBuilder internalBuilder)
        {
            return internalBuilder.ConfiguredHorizontalOptions;
        }
        return defaultAlignment;
    }
}