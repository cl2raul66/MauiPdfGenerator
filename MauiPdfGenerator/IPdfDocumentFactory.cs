using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator;

public interface IPdfDocumentFactory
{
    IPdfDocument CreateDocument();
    IPdfDocument CreateDocument(string path);
}
