using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Sections;

/// <summary>
/// Interface for building the content of a page header.
/// </summary>
public interface IHeaderBuilder
{
    // --- Métodos para añadir contenido ---

    /// <summary>
    /// Adds a paragraph to the header.
    /// </summary>
    /// <param name="paragraphAction">Action to configure the paragraph.</param>
    /// <returns>The header builder instance for chaining.</returns>
    IHeaderBuilder AddParagraph(Action<IPdfParagraphBuilder> paragraphAction); // Placeholder

    // Futuro: AddImage, AddGrid, AddVerticalStackLayout, etc.
    // Futuro: Métodos de configuración específicos del header (ej: Height, Border)
}
