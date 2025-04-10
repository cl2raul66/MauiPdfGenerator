using MauiPdfGenerator.Implementation.Layout.Models;

namespace MauiPdfGenerator.Implementation.Layout.Engine;

/// <summary>
/// Define el contrato para posicionar elementos dentro de un área específica.
/// </summary>
internal interface IArrangeEngine // La interfaz en sí es pública, lo cual es debatible si solo la usa el LayoutEngine interno, pero no causa el error.
{
    /// <summary>
    /// Posiciona un elemento según el contexto proporcionado.
    /// </summary>
    /// <param name="element">El elemento a posicionar</param>
    /// <param name="context">El contexto que incluye el área final y otras infos</param>
    void Arrange(object element, LayoutContext context); // <--- Firma Correcta

    /// <summary>
    /// Determina si un elemento necesita ser reposicionado.
    /// </summary>
    /// <param name="element">El elemento a verificar</param>
    /// <returns>True si el elemento necesita ser reposicionado</returns>
    bool NeedsArrange(object element);
}
