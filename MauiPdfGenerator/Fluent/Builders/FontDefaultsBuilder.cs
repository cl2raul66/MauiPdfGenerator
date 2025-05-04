using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Builders;

internal class FontDefaultsBuilder : IFontDefaultsBuilder
{
    internal string? FamilyName { get; private set; }
    internal float? FontSize { get; private set; }
    internal FontAttributes FontAttribute { get; private set; }

    public IFontDefaultsBuilder Family(string familyName)
    {
        this.FamilyName = familyName;
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
