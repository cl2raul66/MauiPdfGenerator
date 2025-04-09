using MauiPdfGenerator.Common.Geometry;

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Define el contrato para posicionar elementos dentro de un área específica.
/// </summary>
internal interface IArrangeEngine
{
    /// <summary>
    /// Posiciona un elemento en una ubicación específica.
    /// </summary>
    /// <param name="element">El elemento a posicionar</param>
    /// <param name="finalRect">El rectángulo donde se debe posicionar el elemento</param>
    void Arrange(object element, PdfRectangle finalRect);

    /// <summary>
    /// Determina si un elemento necesita ser reposicionado.
    /// </summary>
    /// <param name="element">El elemento a verificar</param>
    /// <returns>True si el elemento necesita ser reposicionado</returns>
    bool NeedsArrange(object element);
}