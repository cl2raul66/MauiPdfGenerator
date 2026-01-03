using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfResourceBuilder : IPdfResourceBuilder
{
    private readonly PdfResourceDictionary _resourceDictionary;

    public PdfResourceBuilder(PdfResourceDictionary resourceDictionary)
    {
        _resourceDictionary = resourceDictionary;
    }

    #region IPdfStylable Style Methods

    public IPdfResourceBuilder Style<TElement>(Action<TElement> setup)
        where TElement : class, IPdfStylable
    {
        string implicitKey = typeof(TElement).FullName ?? typeof(TElement).Name;
        return Style(implicitKey, null, setup);
    }

    public IPdfResourceBuilder Style<TElement>(string key, Action<TElement> setup)
        where TElement : class, IPdfStylable
    {
        return Style(key, null, setup);
    }

    public IPdfResourceBuilder Style<TElement>(string key, PdfStyleIdentifier? basedOn, Action<TElement> setup)
        where TElement : class, IPdfStylable
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
        _resourceDictionary.Add(new PdfStyleIdentifier(key), style);

        return this;
    }

    #endregion

    #region SpanStyle Methods (Backward Compatibility)

    public IPdfResourceBuilder SpanStyle(string key, Action<IPdfSpan> setup)
    {
        return SpanStyle(key, null, setup);
    }

    public IPdfResourceBuilder SpanStyle(string key, PdfStyleIdentifier? basedOn, Action<IPdfSpan> setup)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Style key cannot be null or empty.", nameof(key));
        ArgumentNullException.ThrowIfNull(setup);

        Action<object> safeSetter = (target) =>
        {
            if (target is IPdfSpan typedTarget)
            {
                setup(typedTarget);
            }
        };

        var style = new PdfStyle(typeof(IPdfSpan), basedOn, safeSetter);
        _resourceDictionary.Add(new PdfStyleIdentifier(key), style);

        return this;
    }

    #endregion
}
