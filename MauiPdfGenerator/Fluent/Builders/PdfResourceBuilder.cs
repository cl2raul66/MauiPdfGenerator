using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfResourceBuilder : IPdfResourceBuilder
{
    private readonly PdfResourceDictionary _resourceDictionary;

    public PdfResourceBuilder(PdfResourceDictionary resourceDictionary)
    {
        _resourceDictionary = resourceDictionary;
    }

    public IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup) where TElement : class, IPdfElement<TElement>
    {
        return Style(key, null, setup);
    }

    public IPdfResourceBuilder Style<TElement>(string key, string? basedOn, Action<TElement> setup) where TElement : class, IPdfElement<TElement>
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        ArgumentNullException.ThrowIfNull(setup);

        // Type-safe wrapper for the setter action
        var setter = (object target) =>
        {
            if (target is TElement element)
            {
                setup(element);
            }
        };

        var style = new PdfStyle(typeof(TElement), basedOn, setter);
        _resourceDictionary.Add(key, style);

        return this;
    }
}
