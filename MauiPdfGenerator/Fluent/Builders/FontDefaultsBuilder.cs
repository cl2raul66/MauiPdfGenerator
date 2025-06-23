using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders;

internal class FontDefaultsBuilder : IFontDefaultsBuilder
{
    internal PdfFontIdentifier? FamilyIdentifier { get; private set; }
    internal float? FontSize { get; private set; }
    internal FontAttributes FontAttribute { get; private set; }

    public IFontDefaultsBuilder Family(PdfFontIdentifier? familyIdentifier)
    {
        this.FamilyIdentifier = familyIdentifier;
        return this;
    }

    public IFontDefaultsBuilder Size(float fontSize)
    {
        this.FontSize = fontSize;
        return this;
    }

    public IFontDefaultsBuilder Attributes(FontAttributes attributes)
    {
        this.FontAttribute = attributes;
        return this;
    }
}
