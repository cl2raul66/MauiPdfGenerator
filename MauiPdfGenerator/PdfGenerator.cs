using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Implementation.Builders;

namespace MauiPdfGenerator;

public static class PdfGenerator
{
    /// <summary>
    /// Creates a new PDF document builder instance.
    /// Use a 'using' statement to ensure proper disposal.
    /// </summary>
    /// <returns>An IDocumentBuilder to configure and build the PDF.</returns>
    public static IDocumentBuilder CreateDocument()
    {
        return new DocumentBuilder();
    }
}
