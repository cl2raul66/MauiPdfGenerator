using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

/// <summary>
/// Provides a fluent API for defining resources, such as styles, for a PDF document.
/// </summary>
public interface IPdfResourceBuilder
{
    /// <summary>
    /// Defines a style for a specific element type.
    /// </summary>
    /// <typeparam name="TElement">The type of the element the style targets (e.g., IPdfParagraphStyle).</typeparam>
    /// <param name="key">The unique key to identify the style.</param>
    /// <param name="setup">An action that configures the style's properties.</param>
    /// <returns>The <see cref="IPdfResourceBuilder"/> for chaining.</returns>
    IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup) where TElement : class, IPdfElement<TElement>;

    /// <summary>
    /// Defines a style that inherits from a base style.
    /// </summary>
    /// <typeparam name="TElement">The type of the element the style targets (e.g., IPdfParagraphStyle).</typeparam>
    /// <param name="key">The unique key to identify the style.</param>
    /// <param name="basedOn">The key of the base style to inherit from.</param>
    /// <param name="setup">An action that configures the style's properties, overriding or extending the base style.</param>
    /// <returns>The <see cref="IPdfResourceBuilder"/> for chaining.</returns>
    IPdfResourceBuilder Style<TElement>(string key, string basedOn, Action<TElement> setup) where TElement : class, IPdfElement<TElement>;
}
