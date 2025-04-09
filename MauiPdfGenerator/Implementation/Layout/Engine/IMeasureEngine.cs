using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Define el contrato para medir elementos antes de su disposición final.
/// </summary>
internal interface IMeasureEngine
{
    /// <summary>
    /// Mide un elemento dado las restricciones de espacio.
    /// </summary>
    /// <param name="element">El elemento a medir (ParagraphBuilder, ImageBuilder, etc)</param>
    /// <param name="availableSize">El tamaño disponible para el elemento</param>
    /// <returns>El tamaño que el elemento necesita</returns>
    PdfSize Measure(object element, PdfSize availableSize);

    /// <summary>
    /// Determina si un elemento requiere ser remedido.
    /// </summary>
    /// <param name="element">El elemento a verificar</param>
    /// <returns>True si el elemento necesita ser remedido</returns>
    bool NeedsRemeasure(object element);
}