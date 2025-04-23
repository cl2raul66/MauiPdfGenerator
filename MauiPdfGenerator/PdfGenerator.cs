using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator;

public static class PdfGenerator
{
    public static IPdfDocument CreateDocument()
    {

        return new PdfDocument();
    }

    public static IPdfDocument CreateDocument(string path)
    {

        return new PdfDocument(path);
    }
}
