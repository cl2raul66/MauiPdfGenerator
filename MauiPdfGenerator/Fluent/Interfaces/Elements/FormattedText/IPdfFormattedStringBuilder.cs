using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Elements.FormattedText;

public interface IPdfFormattedStringBuilder
{
    IPdfFormattedStringBuilder Span(string text);

    //IPdfFormattedStringBuilder Span(Action<ISpanBuilder> spanAction);
}
