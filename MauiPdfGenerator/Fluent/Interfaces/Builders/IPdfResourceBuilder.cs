using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfResourceBuilder
{
    IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;

    IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;

    IPdfResourceBuilder Style<TElement>(string key, PdfStyleIdentifier? basedOn, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;
}
