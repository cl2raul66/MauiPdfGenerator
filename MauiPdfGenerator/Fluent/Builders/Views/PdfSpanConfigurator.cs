using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfSpanConfigurator : IPdfSpanConfigurator
{
    private readonly List<PdfSpanBuilder> _spanBuilders = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfSpanConfigurator(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    public IPdfSpan Span(string text)
    {
        var builder = new PdfSpanBuilder(text, _fontRegistry);
        _spanBuilders.Add(builder);
        return builder;
    }

    internal IReadOnlyList<PdfSpanData> BuildSpans() =>
        _spanBuilders.Select(b => b.GetModel()).ToList();
}
