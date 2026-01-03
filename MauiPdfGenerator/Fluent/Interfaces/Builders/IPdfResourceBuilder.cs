using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfResourceBuilder
{
    IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfStylable;

    IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup)
        where TElement : class, IPdfStylable;

    IPdfResourceBuilder Style<TElement>(string key, PdfStyleIdentifier? basedOn, Action<TElement> setup)
        where TElement : class, IPdfStylable;

    IPdfResourceBuilder SpanStyle(string key, Action<IPdfSpan> setup);

    IPdfResourceBuilder SpanStyle(string key, PdfStyleIdentifier? basedOn, Action<IPdfSpan> setup);
}
