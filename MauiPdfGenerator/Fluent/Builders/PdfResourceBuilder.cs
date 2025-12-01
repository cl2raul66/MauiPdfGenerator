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

    public IPdfResourceBuilder Style<TElementStyle>(string key, Action<TElementStyle> setup)
        where TElementStyle : class, IPdfElement<TElementStyle>
    {
        return Style(key, null, setup);
    }

    public IPdfResourceBuilder Style<TElementStyle>(string key, string? basedOn, Action<TElementStyle> setup)
        where TElementStyle : class, IPdfElement<TElementStyle>
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Style key cannot be null or empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(setup);

        Action<object> safeSetter = (target) =>
        {
            if (target is TElementStyle typedTarget)
            {
                setup(typedTarget);
            }
        };

        var style = new PdfStyle(typeof(TElementStyle), basedOn, safeSetter);
        _resourceDictionary.Add(key, style);

        return this;
    }
}
