namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

public interface IPdfFontRegistry
{
    IFontRegistrationOptions Font(string fontFamilyAlias);
}
