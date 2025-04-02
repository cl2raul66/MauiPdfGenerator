using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Interface for building the content and structure of a single page primarily by adding layout containers.
/// </summary>
public interface IPageBuilder
{
    // --- Configuración Específica de Página (Si se necesita en el futuro) ---
    // IPageBuilder Configure(Action<IPageConfigurator> configAction);

    // --- Contenedores de Layout de Nivel Superior ---

    /// <summary>
    /// Adds a Grid layout container to the page.
    /// </summary>
    /// <param name="gridAction">Action to configure the Grid and add elements to it.</param>
    /// <returns>The page builder instance for chaining (to add more top-level layouts, though often a page has one root layout).</returns>
    IPageBuilder AddGrid(Action<IGridBuilder> gridAction);

    /// <summary>
    /// Adds a Vertical Stack Layout container to the page.
    /// Elements are arranged vertically.
    /// </summary>
    /// <param name="vslAction">Action to configure the VerticalStackLayout and add elements to it.</param>
    /// <returns>The page builder instance for chaining.</returns>
    IPageBuilder AddVerticalStackLayout(Action<IVerticalStackLayoutBuilder> vslAction);

    /// <summary>
    /// Adds a Horizontal Stack Layout container to the page.
    /// Elements are arranged horizontally.
    /// </summary>
    /// <param name="hslAction">Action to configure the HorizontalStackLayout and add elements to it.</param>
    /// <returns>The page builder instance for chaining.</returns>
    IPageBuilder AddHorizontalStackLayout(Action<IHorizontalStackLayoutBuilder> hslAction);

    // --- ¿Añadir Elementos Directamente a la Página? ---
    // Podríamos añadir AddParagraph, AddImage aquí, pero es menos común en MAUI
    // donde una página suele tener un único layout raíz. Por ahora, nos enfocamos
    // en añadir layouts.

    // --- Contexto de Página (Lo mantenemos por si acaso) ---
    // int CurrentPageNumber { get; }
    // int TotalPages { get; }
}
// Futuro: IPageConfigurator si se necesita
