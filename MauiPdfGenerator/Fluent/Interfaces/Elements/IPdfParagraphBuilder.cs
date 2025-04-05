using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Builds a paragraph of text, inspired by Microsoft.Maui.Controls.Label formatting.
/// </summary>
public interface IPdfParagraphBuilder : IPdfViewBuilder<IPdfParagraphBuilder> // Inherits common view props like Width, Height, Margin, Alignment, BgColor
{
    // --- Content Definition (Mutually Exclusive) ---

    /// <summary>
    /// Sets the simple text content for the entire paragraph.
    /// Calling this will clear any previously defined formatted text (spans).
    /// </summary>
    /// <param name="text">The plain text content.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder Text(string text);

    /// <summary>
    /// Defines complex formatted text content using styled spans.
    /// Calling this will clear any previously set simple text.
    /// </summary>
    /// <param name="formattedTextAction">Action to build the formatted text by adding spans.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FormattedText(Action<IPdfFormattedTextBuilder> formattedTextAction);

    // --- Formatting Properties (Apply to whole paragraph or as defaults for spans) ---

    /// <summary>
    /// Sets the default font family for the paragraph. Can be overridden by spans.
    /// </summary>
    /// <param name="fontFamily">The font family name (e.g., "Arial", "Times New Roman").</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FontFamily(string fontFamily);

    /// <summary>
    /// Sets the default font size for the paragraph. Can be overridden by spans.
    /// </summary>
    /// <param name="size">The font size.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FontSize(double size);

    /// <summary>
    /// Sets the default text color for the paragraph. Can be overridden by spans.
    /// </summary>
    /// <param name="color">The text color.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder TextColor(Color color);

    /// <summary>
    /// Sets the default font attributes (e.g., Bold, Italic) for the paragraph. Can be combined or overridden by spans.
    /// </summary>
    /// <param name="attributes">The font attributes.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder FontAttributes(PdfFontAttributes attributes);

    /// <summary>
    /// Sets the default text decorations (e.g., Underline, Strikethrough) for the paragraph. Can be combined or overridden by spans.
    /// </summary>
    /// <param name="decorations">The text decorations.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder TextDecorations(PdfTextDecorations decorations);

    /// <summary>
    /// Sets the horizontal alignment of the text lines within the paragraph's bounds.
    /// </summary>
    /// <param name="alignment">The horizontal text alignment.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder HorizontalTextAlignment(PdfTextAlignment alignment);

    /// <summary>
    /// Sets the vertical alignment of the text block within its allocated vertical space.
    /// (Note: Often less impactful for single paragraphs unless they have explicit height).
    /// </summary>
    /// <param name="alignment">The vertical text alignment.</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder VerticalTextAlignment(PdfTextAlignment alignment);

    /// <summary>
    /// Sets the line height multiplier for the paragraph. Affects spacing between lines.
    /// </summary>
    /// <param name="multiplier">The line height multiplier (e.g., 1.0 for normal, 1.5 for 1.5x spacing).</param>
    /// <returns>The builder instance for chaining.</returns>
    IPdfParagraphBuilder LineHeight(double multiplier);

    /// <summary>
    /// Sets uniform padding inside the paragraph borders.
    /// </summary>
    IPdfParagraphBuilder Padding(double uniformPadding);

    /// <summary>
    /// Sets horizontal and vertical padding inside the paragraph borders.
    /// </summary>
    IPdfParagraphBuilder Padding(double horizontal, double vertical);

    /// <summary>
    /// Sets individual padding values inside the paragraph borders.
    /// </summary>
    IPdfParagraphBuilder Padding(double left, double top, double right, double bottom);
}
