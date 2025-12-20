using MauiPdfGenerator.Common;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Builders;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MauiPdfGenerator.Tests.MauiPdfGenerator.Common;

public class PdfDocumentFactoryTests 
{
    private readonly PdfFontRegistryBuilder _fontRegistry;
    private readonly Mock<ILoggerFactory> _mockLoggerFactory;
    private readonly Mock<IDiagnosticSink> _mockDiagnosticSink;
    private readonly Mock<IPdfCoreGenerator> _mockCoreGenerator;
    private readonly PdfDocumentFactory _factory;

    public PdfDocumentFactoryTests()
    {
        _fontRegistry = new PdfFontRegistryBuilder();
        _mockLoggerFactory = new Mock<ILoggerFactory>();
        _mockDiagnosticSink = new Mock<IDiagnosticSink>();
        _mockCoreGenerator = new Mock<IPdfCoreGenerator>();

        _factory = new PdfDocumentFactory(
            _fontRegistry,
            _mockLoggerFactory.Object,
            _mockDiagnosticSink.Object,
            _mockCoreGenerator.Object);
    }

    [Fact]
    public void CreateDocument_WithoutPath_ReturnsPdfDocumentBuilder()
    {
        var result = _factory.CreateDocument();

        Assert.IsType<PdfDocumentBuilder>(result);
    }

    [Fact]
    public void CreateDocument_WithPath_ReturnsPdfDocumentBuilder()
    {
        const string path = "test.pdf";
        var result = _factory.CreateDocument(path); 

        Assert.IsType<PdfDocumentBuilder>(result);
    }
}