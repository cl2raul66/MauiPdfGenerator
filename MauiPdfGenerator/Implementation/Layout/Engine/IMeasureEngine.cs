using MauiPdfGenerator.Common.Geometry; // Para PdfSize
using MauiPdfGenerator.Implementation.Layout.Models; // Para LayoutContext

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Define el contrato para medir elementos antes de su disposición final.
/// </summary>
internal interface IMeasureEngine // Puede ser 'internal' si solo la usa el engine
{
    /// <summary>
    /// Mide un elemento dado el contexto de layout (que incluye el espacio disponible y restricciones).
    /// </summary>
    /// <param name="element">El elemento a medir (ParagraphBuilder, ImageBuilder, etc.)</param>
    /// <param name="context">El contexto de layout que proporciona el área y restricciones.</param>
    /// <returns>El tamaño que el elemento necesita (PdfSize).</returns>
    PdfSize Measure(object element, LayoutContext context); // <-- Firma CORRECTA, usa LayoutContext

    /// <summary>
    /// Determina si un elemento requiere ser remedido.
    /// </summary>
    /// <param name="element">El elemento a verificar.</param>
    /// <returns>True si el elemento necesita ser remedido.</returns>
    bool NeedsRemeasure(object element);
}
