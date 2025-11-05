using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfFontDefaultsBuilder : IPdfFontDefaultsBuilder
{
    internal PdfFontIdentifier? FamilyIdentifier { get; private set; }
    internal float? FontSize { get; private set; }
    internal FontAttributes FontAttribute { get; private set; }

    public IPdfFontDefaultsBuilder Family(PdfFontIdentifier? familyIdentifier)
    {
        this.FamilyIdentifier = familyIdentifier;
        return this;
    }

    public IPdfFontDefaultsBuilder Size(float fontSize)
    {
        this.FontSize = fontSize;
        return this;
    }

    public IPdfFontDefaultsBuilder Attributes(FontAttributes attributes)
    {
        this.FontAttribute = attributes;
        return this;
    }
}
