using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Interface for building a styled inline span of text within a paragraph.
/// </summary>
public interface IPdfSpanBuilder
{
    /// <summary>
    /// Sets the text content for this span.
    /// </summary>
    /// <param name="text">The text content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder Text(string text);

    /// <summary>
    /// Sets the text color for this span. Overrides paragraph color if set.
    /// </summary>
    /// <param name="color">The text color.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder TextColor(Color color);

    /// <summary>
    /// Sets the font family for this span. Overrides paragraph font if set.
    /// </summary>
    /// <param name="fontFamily">The font family name.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder FontFamily(string fontFamily);

    /// <summary>
    /// Sets the font size for this span. Overrides paragraph font size if set.
    /// </summary>
    /// <param name="size">The font size.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder FontSize(double size);

    /// <summary>
    /// Sets the font attributes (e.g., Bold, Italic) for this span.
    /// </summary>
    /// <param name="attributes">The font attributes.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder FontAttributes(PdfFontAttributes attributes);

    /// <summary>
    /// Sets the text decorations (e.g., Underline, Strikethrough) for this span.
    /// </summary>
    /// <param name="decorations">The text decorations.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder TextDecorations(PdfTextDecorations decorations);

    /// <summary>
    /// Sets the background color specifically for this span (e.g., for highlighting).
    /// </summary>
    /// <param name="color">The background color.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder BackgroundColor(Color color);

    /// <summary>
    /// Sets the line height multiplier for this span. Affects the line containing the span.
    /// </summary>
    /// <param name="multiplier">The line height multiplier (e.g., 1.0 for normal, 1.5 for 1.5x spacing).</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfSpanBuilder LineHeight(double multiplier);
}
