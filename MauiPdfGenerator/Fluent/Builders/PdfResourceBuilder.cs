using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfResourceBuilder : IPdfResourceBuilder
{
    private readonly PdfResourceDictionary _resourceDictionary;

    public PdfResourceBuilder(PdfResourceDictionary resourceDictionary)
    {
        _resourceDictionary = resourceDictionary;
    }

    public IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        string implicitKey = typeof(TElement).FullName ?? typeof(TElement).Name;
        return Style(implicitKey, null, setup);
    }

    public IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        return Style(key, null, setup);
    }

    public IPdfResourceBuilder Style<TElement>(string key, string? basedOn, Action<TElement> setup)
        where TElement : class, IPdfElement<TElement>
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Style key cannot be null or empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(setup);

        Action<object> safeSetter = (target) =>
        {
            if (target is TElement typedTarget)
            {
                setup(typedTarget);
            }
        };

        var style = new PdfStyle(typeof(TElement), basedOn, safeSetter);
        _resourceDictionary.Add(key, style);

        return this;
    }
}
