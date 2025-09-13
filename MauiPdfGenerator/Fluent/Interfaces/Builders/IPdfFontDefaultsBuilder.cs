using MauiPdfGenerator.Fluent.Models; 

namespace MauiPdfGenerator.Fluent.Interfaces.Builders;

public interface IPdfFontDefaultsBuilder
{
    IPdfFontDefaultsBuilder Family(PdfFontIdentifier? familyIdentifier);
    IPdfFontDefaultsBuilder Size(float fontSize);
    IPdfFontDefaultsBuilder Attributes(FontAttributes attributes);
}
