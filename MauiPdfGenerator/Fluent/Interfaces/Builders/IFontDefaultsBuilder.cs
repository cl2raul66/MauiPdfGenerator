using MauiPdfGenerator.Fluent.Models; 

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IFontDefaultsBuilder
{
    IFontDefaultsBuilder Family(PdfFontIdentifier? familyIdentifier);
    IFontDefaultsBuilder Size(float fontSize);
    IFontDefaultsBuilder Attributes(FontAttributes attributes);
}
