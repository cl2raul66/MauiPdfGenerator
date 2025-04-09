using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Implementation.Builders;

// --- Implementations for Formatted Text / Spans ---

/// <summary>
/// Internal implementation for building formatted text with spans.
/// </summary>
internal class FormattedTextBuilder : IPdfFormattedTextBuilder
{
    internal List<SpanBuilder> Spans { get; } = new List<SpanBuilder>();

    public IPdfFormattedTextBuilder AddSpan(Action<IPdfSpanBuilder> spanAction)
    {
        if (spanAction == null) throw new ArgumentNullException(nameof(spanAction));
        var spanBuilder = new SpanBuilder();
        spanAction(spanBuilder);
        // Only add span if it actually contains text
        if (!string.IsNullOrEmpty(spanBuilder.ConfiguredText))
        {
            Spans.Add(spanBuilder);
        }
        return this;
    }
}

// Fin namespace
