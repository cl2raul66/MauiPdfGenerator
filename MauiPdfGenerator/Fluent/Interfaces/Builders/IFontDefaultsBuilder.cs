namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IFontDefaultsBuilder
{
    IFontDefaultsBuilder Family(string familyName);

    IFontDefaultsBuilder Size(float fontSize);

    IFontDefaultsBuilder Attributes(FontAttributes attributes);
}
