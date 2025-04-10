using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Implementation.Builders;
using MauiPdfGenerator.Implementation.Layout.Engine;
using MauiPdfGenerator.Implementation.Layout.Models;
using MauiPdfGenerator.Implementation.Layout;

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
        var padding = builder.ConfiguredPadding;
        var spacing = builder.ConfiguredSpacing;
        var children = builder.ConfiguredChildren;

        // Calcular el espacio disponible para los hijos considerando el padding
        var availableChildWidth = context.AvailableArea.Width - padding.Left - padding.Right;
        var availableChildHeight = context.AvailableArea.Height - padding.Top - padding.Bottom;
        var childAvailableSize = new PdfSize(availableChildWidth, availableChildHeight);

        double totalHeight = padding.Top + padding.Bottom;
        double maxWidth = 0;

        // Medir cada hijo
        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var childSize = _measureEngine.Measure(child, childAvailableSize);

            // Actualizar dimensiones
            maxWidth = Math.Max(maxWidth, childSize.Width);
            totalHeight += childSize.Height;

            // Agregar espaciado entre elementos (excepto después del último)
            if (i < children.Count - 1)
            {
                totalHeight += spacing;
            }
        }

        // El ancho final es el máximo de los hijos más el padding
        maxWidth += padding.Left + padding.Right;

        // Ajustar según las restricciones
        if (builder.ConfiguredWidth.HasValue)
        {
            maxWidth = builder.ConfiguredWidth.Value;
        }

        if (builder.ConfiguredHeight.HasValue)
        {
            totalHeight = builder.ConfiguredHeight.Value;
        }

        return new PdfSize(maxWidth, totalHeight);
    }

    public void Arrange(VerticalStackLayoutBuilder builder, LayoutContext context)
    {
        var padding = builder.ConfiguredPadding; // Usa Microsoft.Maui.Thickness ahora
        var spacing = builder.ConfiguredSpacing;
        var children = builder.ConfiguredChildren;
        var availableArea = context.AvailableArea; // Coordenadas PDF (LL)

        // Posición inicial Y (superior del área de contenido interna, en coordenadas PDF)
        double currentY = availableArea.Y + availableArea.Height - padding.Top;
        var childLeft = availableArea.X + padding.Left;
        var availableChildWidth = availableArea.Width - padding.Left - padding.Right;

        // Validar ancho disponible
        if (availableChildWidth < 0) availableChildWidth = 0;

        // Posicionar cada hijo
        foreach (var child in children)
        {
            // Medir al hijo con el ancho disponible (altura infinita para que reporte su necesidad)
            // Usamos MaxValue, pero podríamos usar la altura restante si quisiéramos limitar.
            var childAvailableSize = new PdfSize(availableChildWidth, double.PositiveInfinity);
            var childMeasuredSize = _measureEngine.Measure(child, childAvailableSize);

            // Calcular la posición X del hijo (origen izquierdo) según alineación
            double childX = childLeft;
            if (childMeasuredSize.Width < availableChildWidth) // Solo aplicar alineación si hay espacio extra
            {
                var horizontalAlignment = GetChildHorizontalAlignment(child, builder.ConfiguredHorizontalOptions); // Helper para obtener alineación del hijo o default

                if (horizontalAlignment == PdfHorizontalAlignment.Center)
                {
                    childX = childLeft + (availableChildWidth - childMeasuredSize.Width) / 2;
                }
                else if (horizontalAlignment == PdfHorizontalAlignment.End)
                {
                    childX = childLeft + availableChildWidth - childMeasuredSize.Width;
                }
                // PdfHorizontalAlignment.Fill o .Start se manejan con childX = childLeft y el Arrange del hijo debe llenar si es Fill.
            }

            // Calcular la posición Y inferior del hijo (en coordenadas PDF)
            double childBottomY = currentY - childMeasuredSize.Height;

            // Crear el rectángulo final para el hijo en coordenadas PDF [LLx, LLy, Width, Height]
            var childFinalRect = new PdfRectangle(
                childX,
                childBottomY,
                childMeasuredSize.Width, // Usar el ancho medido
                childMeasuredSize.Height
            );

            // *** Crear un nuevo contexto para el hijo ***
            // Es importante pasar el rectángulo final calculado como la nueva área disponible
            // para que el hijo se posicione correctamente dentro de él.
            var childContext = context.CreateChildContext(childFinalRect); // Usar el rect final como nueva área

            // Posicionar el hijo usando el nuevo contexto
            _arrangeEngine.Arrange(child, childContext); // Pasamos el contexto, no solo el rect

            // Actualizar la posición Y superior para el siguiente elemento
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