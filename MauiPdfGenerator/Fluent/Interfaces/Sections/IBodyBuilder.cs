using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Sections;

/// <summary>
/// Interface for building the content of a page body.
/// </summary>
public interface IBodyBuilder
{
    // --- Métodos para añadir contenido ---

    /// <summary>
    /// Adds a paragraph to the body.
    /// </summary>
    /// <param name="paragraphAction">Action to configure the paragraph.</param>
    /// <returns>The body builder instance for chaining.</returns>
    IBodyBuilder AddParagraph(Action<IParagraphBuilder> paragraphAction); // Placeholder

    // Futuro: AddImage, AddGrid, AddVerticalStackLayout, AddTable, AddBulletList, etc.
}
