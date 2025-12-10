using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfResourceBuilder
{
    IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;

    IPdfResourceBuilder Style<TElement>(PdfStyleIdentifier key, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;

    IPdfResourceBuilder Style<TElement>(PdfStyleIdentifier key, PdfStyleIdentifier basedOn, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>;
}
