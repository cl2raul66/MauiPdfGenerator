using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Models; // Asegurar using

namespace MauiPdfGenerator.Fluent.Builders;

internal class FontDefaultsBuilder : IFontDefaultsBuilder
{
    // FamilyIdentifier ahora es nullable
    internal PdfFontIdentifier? FamilyIdentifier { get; private set; }
    internal float? FontSize { get; private set; }
    internal FontAttributes FontAttribute { get; private set; }

    // Acepta PdfFontIdentifier?
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
