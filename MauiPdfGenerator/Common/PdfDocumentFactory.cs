using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Common;

internal class PdfDocumentFactory : IPdfDocumentFactory
{
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly ILoggerFactory _loggerFactory;

    public PdfDocumentFactory(PdfFontRegistryBuilder fontRegistry, ILoggerFactory loggerFactory)
    {
        _fontRegistry = fontRegistry;
        _loggerFactory = loggerFactory;
    }

    public IPdfDocument CreateDocument()
    {
        return new PdfDocumentBuilder(_fontRegistry, _loggerFactory);
    }

    public IPdfDocument CreateDocument(string path)
    {
        return new PdfDocumentBuilder(_fontRegistry, _loggerFactory, path);
    }
}
