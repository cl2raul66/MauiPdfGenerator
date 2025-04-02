using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Sections;

/// <summary>
/// Interface for building the content of a page footer.
/// </summary>
public interface IFooterBuilder
{
    // --- Métodos para añadir contenido ---

    /// <summary>
    /// Adds a paragraph to the footer.
    /// </summary>
    /// <param name="paragraphAction">Action to configure the paragraph.</param>
    /// <returns>The footer builder instance for chaining.</returns>
    IFooterBuilder AddParagraph(Action<IParagraphBuilder> paragraphAction); 

    // Futuro: AddImage, AddGrid, AddVerticalStackLayout, etc.
    // Futuro: Métodos de configuración específicos del footer (ej: Height, Border)
    // Futuro: Acceso a contexto de paginación (CurrentPage, TotalPages)
}
