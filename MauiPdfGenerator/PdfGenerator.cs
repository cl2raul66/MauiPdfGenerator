using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator;

public static class PdfGenerator
{
    public static IPdfDocument CreateDocument()
    {

        return new PdfDocumentBuilder();
    }

    public static IPdfDocument CreateDocument(string path)
    {

        return new PdfDocumentBuilder(path);
    }
}
