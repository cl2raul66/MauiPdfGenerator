using System.Text;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Builders.Views;
using MauiPdfGenerator.Fluent.Interfaces.Views;

namespace MauiPdfGenerator.Fluent.Utils;

internal class PdfSpanConfigurator : IPdfSpanText
{
    private readonly List<SpanItem> _items = [];
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly PdfResourceDictionary? _resourceDictionary;

    private class SpanItem
    {
        public required string Text { get; init; }
        public required PdfSpanBuilder Builder { get; init; }
    }

    public PdfSpanConfigurator(PdfFontRegistryBuilder fontRegistry, PdfResourceDictionary? resourceDictionary = null)
    {
        _fontRegistry = fontRegistry;
        _resourceDictionary = resourceDictionary;
    }

    public IPdfBuildableSpan Text(string text)
    {
        var builder = new PdfSpanBuilder(text.Length, _fontRegistry, _resourceDictionary);
        _items.Add(new SpanItem { Text = text, Builder = builder });
        return builder;
    }

    internal (string Text, List<PdfSpanData> Spans) Build()
    {
        var concatenatedText = new StringBuilder();
        var spans = new List<PdfSpanData>();
        int currentIndex = 0;

        foreach (var item in _items)
        {
            var spanData = item.Builder.GetModel();
            spanData.StartIndex = currentIndex;
            currentIndex += item.Text.Length;
            spanData.EndIndex = currentIndex;

            spans.Add(spanData);
            concatenatedText.Append(item.Text);
        }

        return (concatenatedText.ToString(), spans);
    }
}
