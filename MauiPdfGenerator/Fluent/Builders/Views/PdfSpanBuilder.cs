using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfSpanBuilder : IPdfSpan
{
    private readonly PdfSpanData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfSpanBuilder(string text, PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfSpanData(text);
        _fontRegistry = fontRegistry;
    }

    internal PdfSpanData GetModel() => _model;

    public IPdfSpan FontFamily(PdfFontIdentifier? family)
    {
        if (family.HasValue)
        {
            _model.FontFamilyProp.Set(family, PdfPropertyPriority.Local);
            _model.ResolvedFontRegistration = _fontRegistry.GetFontRegistration(family.Value);
        }
        return this;
    }

    public IPdfSpan FontSize(float size)
    {
        _model.FontSizeProp.Set(size, PdfPropertyPriority.Local);
        return this;
    }

    public IPdfSpan TextColor(Color color)
    {
        _model.TextColorProp.Set(color, PdfPropertyPriority.Local);
        return this;
    }

    public IPdfSpan FontAttributes(FontAttributes attributes)
    {
        _model.FontAttributesProp.Set(attributes, PdfPropertyPriority.Local);
        return this;
    }

    public IPdfSpan TextDecorations(TextDecorations decorations)
    {
        _model.TextDecorationsProp.Set(decorations, PdfPropertyPriority.Local);
        return this;
    }

    public IPdfSpan TextTransform(TextTransform transform)
    {
        _model.TextTransformProp.Set(transform, PdfPropertyPriority.Local);
        return this;
    }
}
