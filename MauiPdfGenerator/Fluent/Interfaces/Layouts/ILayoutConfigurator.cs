namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

/// <summary>
/// Interface for common layout configuration properties like Padding and BackgroundColor.
/// </summary>
/// <typeparam name="TBuilder">The specific type of the builder implementing this interface (for fluent return type).</typeparam>
public interface ILayoutConfigurator<TBuilder> where TBuilder : ILayoutConfigurator<TBuilder>
{
    /// <summary>
    /// Sets uniform padding for all sides of the layout container.
    /// </summary>
    /// <param name="uniformPadding">The padding value.</param>
    /// <returns>The layout builder instance for chaining.</returns>
    TBuilder Padding(float uniformPadding);

    /// <summary>
    /// Sets horizontal and vertical padding for the layout container.
    /// </summary>
    /// <param name="horizontal">Padding for left and right sides.</param>
    /// <param name="vertical">Padding for top and bottom sides.</param>
    /// <returns>The layout builder instance for chaining.</returns>
    TBuilder Padding(float horizontal, float vertical);

    /// <summary>
    /// Sets individual padding values for each side of the layout container.
    /// </summary>
    /// <param name="left">Left padding.</param>
    /// <param name="top">Top padding.</param>
    /// <param name="right">Right padding.</param>
    /// <param name="bottom">Bottom padding.</param>
    /// <returns>The layout builder instance for chaining.</returns>
    TBuilder Padding(float left, float top, float right, float bottom);

    // Futuro: Podría aceptar un objeto PdfPadding si lo creamos.
    // TBuilder Padding(PdfPadding padding);

    /// <summary>
    /// Sets the background color of the layout container.
    /// </summary>
    /// <param name="color">The background color.</param>
    /// <returns>The layout builder instance for chaining.</returns>
    TBuilder BackgroundColor(Color color); // Asumiendo que PdfColor existe en Models
}
