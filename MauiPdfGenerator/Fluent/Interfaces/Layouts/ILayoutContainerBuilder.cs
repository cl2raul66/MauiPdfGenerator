namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Base interface for layout builders that can contain other elements.
/// Defines the common 'Add...' methods.
/// </summary>
public interface ILayoutContainerBuilder
{
    // --- Añadir Layouts Anidados ---

    /// <summary>
    /// Adds a nested Grid layout container.
    /// </summary>
    ILayoutContainerBuilder AddGrid(Action<IGridBuilder> gridAction);

    /// <summary>
    /// Adds a nested Vertical Stack Layout container.
    /// </summary>
    ILayoutContainerBuilder AddVerticalStackLayout(Action<IVerticalStackLayoutBuilder> vslAction);

    /// <summary>
    /// Adds a nested Horizontal Stack Layout container.
    /// </summary>
    ILayoutContainerBuilder AddHorizontalStackLayout(Action<IHorizontalStackLayoutBuilder> hslAction);

    // --- Añadir Elementos de Contenido ---

    /// <summary>
    /// Adds a paragraph of text.
    /// </summary>
    ILayoutContainerBuilder AddParagraph(Action<IParagraphBuilder> paragraphAction);

    /// <summary>
    /// Adds an image.
    /// </summary>
    ILayoutContainerBuilder AddImage(Action<IImageBuilder> imageAction);

    // --- Otros Elementos (Ejemplos) ---
    /// <summary>
    /// Adds a table.
    /// </summary>
    ILayoutContainerBuilder AddTable(Action<ITableBuilder> tableAction); // Placeholder

    /// <summary>
    /// Adds a bulleted list.
    /// </summary>
    ILayoutContainerBuilder AddBulletList(Action<IBulletListBuilder> listAction); // Placeholder

    // Nota: El tipo de retorno es ILayoutContainerBuilder para permitir
    // añadir múltiples elementos seguidos al mismo contenedor.
    // Las implementaciones concretas devolverán 'this'.
}
