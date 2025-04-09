using MauiPdfGenerator.Common.Geometry;
using MauiPdfGenerator.Core.Content;

namespace MauiPdfGenerator.Implementation.Layout.Models;

/// <summary>
/// Proporciona contexto durante el proceso de layout.
/// </summary>
internal class LayoutContext
{
    /// <summary>
    /// El área disponible actual para el layout
    /// </summary>
    public PdfRectangle AvailableArea { get; }

    /// <summary>
    /// Las restricciones aplicadas al layout actual
    /// </summary>
    public LayoutConstraints Constraints { get; }

    /// <summary>
    /// El content stream donde se dibujará el contenido
    /// </summary>
    public PdfContentStream ContentStream { get; }

    /// <summary>
    /// Los recursos disponibles para el layout actual
    /// </summary>
    public PdfResources Resources { get; }

    /// <summary>
    /// El nivel de anidamiento actual en el árbol de layout
    /// </summary>
    public int Depth { get; }

    public LayoutContext(
        PdfRectangle availableArea,
        LayoutConstraints constraints,
        PdfContentStream contentStream,
        PdfResources resources,
        int depth = 0)
    {
        AvailableArea = availableArea;
        Constraints = constraints;
        ContentStream = contentStream;
        Resources = resources;
        Depth = depth;
    }

    /// <summary>
    /// Crea un nuevo contexto para un elemento hijo
    /// </summary>
    public LayoutContext CreateChildContext(PdfRectangle childArea, LayoutConstraints? childConstraints = null)
    {
        return new LayoutContext(
            childArea,
            childConstraints ?? Constraints,
            ContentStream,
            Resources,
            Depth + 1
        );
    }
}