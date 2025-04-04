namespace MauiPdfGenerator.Fluent.Interfaces;

/// <summary>
/// Base interface for all Fluent PDF view builders (elements and layouts).
/// Provides common configuration methods relevant for PDF generation.
/// </summary>
/// <typeparam name="TBuilder">The concrete type of the builder implementing this interface.</typeparam>
public interface IPdfViewBuilder<TBuilder> where TBuilder : IPdfViewBuilder<TBuilder>
{
    /// <summary>
    /// Sets an explicit width for the view.
    /// Note: Layout containers might override or influence this.
    /// </summary>
    /// <param name="width">The desired width.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder Width(double width);

    /// <summary>
    /// Sets an explicit height for the view.
    /// Note: Layout containers might override or influence this.
    /// </summary>
    /// <param name="height">The desired height.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder Height(double height);

    /// <summary>
    /// Sets uniform margin for all sides.
    /// </summary>
    /// <param name="uniformMargin">The margin value.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder Margin(double uniformMargin);

    /// <summary>
    /// Sets horizontal and vertical margins.
    /// </summary>
    /// <param name="horizontal">Left and right margin.</param>
    /// <param name="vertical">Top and bottom margin.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder Margin(double horizontal, double vertical);

    /// <summary>
    /// Sets individual margins for each side.
    /// </summary>
    /// <param name="left">Left margin.</param>
    /// <param name="top">Top margin.</param>
    /// <param name="right">Right margin.</param>
    /// <param name="bottom">Bottom margin.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder Margin(double left, double top, double right, double bottom);

    /// <summary>
    /// Sets the horizontal alignment of the view within its allocated space.
    /// </summary>
    /// <param name="alignment">The horizontal alignment option.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder HorizontalAlignment(HorizontalAlignment alignment);

    /// <summary>
    /// Sets the vertical alignment of the view within its allocated space.
    /// </summary>
    /// <param name="alignment">The vertical alignment option.</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder VerticalAlignment(VerticalAlignment alignment);

    /// <summary>
    /// Sets the background color of the view.
    /// </summary>
    /// <param name="color">The background color (from Microsoft.Maui.Graphics).</param>
    /// <returns>The builder instance for chaining.</returns>
    TBuilder BackgroundColor(Color color);

    // --- Internal Use / Advanced ---
    // This method might be needed by extensions or internal logic
    // to store associated data (like Grid Row/Column).
    // Alternatively, concrete builders hold this state.
    // TBuilder SetProperty(object key, object value);
    // object GetProperty(object key);
}
