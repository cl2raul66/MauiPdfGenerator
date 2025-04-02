using MauiPdfGenerator.Fluent.Interfaces;

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
        // Aquí instanciarías y devolverías tu implementación interna
        // Por ahora, solo la firma:
        // return new DocumentBuilder();
        throw new NotImplementedException("Implementación del DocumentBuilder pendiente.");
    }
}
