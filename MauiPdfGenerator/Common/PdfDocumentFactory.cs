using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Fluent.Interfaces;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Common;

internal class PdfDocumentFactory : IPdfDocumentFactory
{
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDiagnosticSink _diagnosticSink;
    private readonly IPdfCoreGenerator _coreGenerator;

    public PdfDocumentFactory(PdfFontRegistryBuilder fontRegistry, ILoggerFactory loggerFactory, IDiagnosticSink diagnosticSink, IPdfCoreGenerator coreGenerator)
    {
        _fontRegistry = fontRegistry;
        _loggerFactory = loggerFactory;
        _diagnosticSink = diagnosticSink;
        _coreGenerator = coreGenerator;
    }

    public IPdfDocument CreateDocument()
    {
        return new PdfDocumentBuilder(_fontRegistry, _loggerFactory, _diagnosticSink, _coreGenerator);
    }

    public IPdfDocument CreateDocument(string path)
    {
        return new PdfDocumentBuilder(_fontRegistry, _loggerFactory, _diagnosticSink, _coreGenerator, path);
    }
}
