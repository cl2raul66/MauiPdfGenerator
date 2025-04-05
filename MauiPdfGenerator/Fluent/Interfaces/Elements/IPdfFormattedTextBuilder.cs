namespace MauiPdfGenerator.Fluent.Interfaces.Elements;

/// <summary>
/// Interface for building formatted text consisting of multiple styled spans.
/// Used within the IPdfParagraphBuilder.FormattedText() method.
/// </summary>
public interface IPdfFormattedTextBuilder
{
    /// <summary>
    /// Adds a styled inline text span to the formatted text.
    /// </summary>
    /// <param name="spanAction">Action to configure the span's properties.</param>
    /// <returns>The formatted text builder instance for chaining more spans.</returns>
    IPdfFormattedTextBuilder AddSpan(Action<IPdfSpanBuilder> spanAction);
}
