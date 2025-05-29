using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator;

internal class PdfDocumentFactory : IPdfDocumentFactory
{
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfDocumentFactory(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    public IPdfDocument CreateDocument()
    {
        return new PdfDocumentBuilder(_fontRegistry);
    }

    public IPdfDocument CreateDocument(string path)
    {
        return new PdfDocumentBuilder(_fontRegistry, path);
    }
}
