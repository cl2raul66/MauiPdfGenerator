using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Interfaces.Configuration;

public interface IPdfFontRegistry
{
    IFontRegistrationOptions Font(PdfFontIdentifier fontIdentifier);
}
