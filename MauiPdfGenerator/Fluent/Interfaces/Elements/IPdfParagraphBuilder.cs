namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Builds a paragraph of text.
/// </summary>
public interface IPdfParagraphBuilder : IPdfViewBuilder<IPdfParagraphBuilder>
{
    /// <summary>
    /// Sets the text content of the paragraph.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder Text(string text);

    /// <summary>
    /// Sets the font size.
    /// </summary>
    /// <param name="size">The font size.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FontSize(double size);

    /// <summary>
    /// Sets the text color.
    /// </summary>
    /// <param name="color">The text color.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder TextColor(Color color);

    /// <summary>
    /// Sets the font attributes (e.g., Bold, Italic).
    /// </summary>
    /// <param name="attributes">The font attributes.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FontAttributes(FontAttributes attributes);

    // --- Other common paragraph properties ---
    // IPdfParagraphBuilder FontFamily(string fontFamily);
    // IPdfParagraphBuilder LineHeight(double multiplier);
    // IPdfParagraphBuilder TextAlignment(TextAlignment alignment); // Could be HorizontalAlignment from base?
    // IPdfParagraphBuilder AddSpan(Action<IPdfSpanBuilder> spanAction); // For inline formatting
}
// Placeholder for potential inline formatting
// public interface IPdfSpanBuilder { /* TextColor, FontAttributes, Text */ }
