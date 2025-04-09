using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Implementation.Layout.Models;

/// <summary>
/// Encapsula los datos necesarios para una solicitud de layout.
/// </summary>
internal class LayoutRequest
{
    /// <summary>
    /// El elemento que necesita ser medido y posicionado
    /// </summary>
    public object Element { get; }

    /// <summary>
    /// El área disponible para el elemento
    /// </summary>
    public PdfRectangle AvailableArea { get; }

    /// <summary>
    /// Las restricciones específicas para el layout (como márgenes, padding, etc)
    /// </summary>
    public LayoutConstraints Constraints { get; }

    public LayoutRequest(object element, PdfRectangle availableArea, LayoutConstraints? constraints = null)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        AvailableArea = availableArea;
        Constraints = constraints ?? LayoutConstraints.None;
    }
}