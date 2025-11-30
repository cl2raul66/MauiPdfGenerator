using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Diagnostics.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using Microsoft.Extensions.Logging;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfDocumentBuilder : IPdfDocument
{
    private string? _filePath;
    private readonly PdfConfigurationBuilder _configurationBuilder;
    private readonly List<IPdfPageBuilder> _pages;
    private readonly IPdfCoreGenerator _pdfGenerationService;
    private readonly ILogger _logger;
    private readonly IDiagnosticSink _diagnosticSink;

    public PdfDocumentBuilder(PdfFontRegistryBuilder fontRegistry, ILoggerFactory loggerFactory, IDiagnosticSink diagnosticSink, IPdfCoreGenerator pdfGenerationService, string? defaultPath = null)
    {
        _filePath = defaultPath;
        _pages = [];
        _configurationBuilder = new PdfConfigurationBuilder(fontRegistry);
        _pdfGenerationService = pdfGenerationService;
        _logger = loggerFactory.CreateLogger<IPdfDocument>();
        _diagnosticSink = diagnosticSink;
    }

    public IPdfDocument Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)
    {
        ArgumentNullException.ThrowIfNull(documentConfigurator);
        documentConfigurator(_configurationBuilder);
        return this;
    }

    public IPdfDocument Resources(Action<IPdfResourceBuilder> resourceBuilderAction)
    {
        ArgumentNullException.ThrowIfNull(resourceBuilderAction);
        var resourceBuilder = new PdfResourceBuilder(_configurationBuilder.ResourceDictionary);
        resourceBuilderAction(resourceBuilder);
        return this;
    }

    // CORRECCIÓN: El tipo de retorno ahora es IPdfConfigurablePage<TLayout> para coincidir con la interfaz.
    public IPdfConfigurablePage<TLayout> ContentPage<TLayout>() where TLayout : class
    {
        var pageBuilder = new PdfContentPageBuilder<TLayout>(this, _configurationBuilder, _configurationBuilder.FontRegistry);
        _pages.Add(pageBuilder);
        return pageBuilder;
    }

    // CORRECCIÓN: El tipo de retorno ahora es IPdfConfigurablePage<IPdfVerticalStackLayout>.
    public IPdfConfigurablePage<IPdfVerticalStackLayout> ContentPage()
    {
        return ContentPage<IPdfVerticalStackLayout>();
    }

    public Task SaveAsync()
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            throw new InvalidOperationException("Default file path was not specified during document creation. Use SaveAsync(path) or provide a path when calling CreateDocument.");
        }
        return SaveAsync(_filePath);
    }

    public async Task SaveAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "File path cannot be null or empty.");
        }

        var allElements = GetAllElements();
        var styleResolver = new StyleResolver(_configurationBuilder.ResourceDictionary, _diagnosticSink);
        styleResolver.ApplyStyles(allElements);

        var pageDataList = new List<PdfPageData>();
        foreach (var pageBuilder in _pages)
        {
            if (pageBuilder is IPdfContentPageBuilder contentPageBuilder)
            {
                var pageData = new PdfPageData(
                    contentPageBuilder.GetEffectivePageSize(),
                    contentPageBuilder.GetEffectivePageOrientation(),
                    contentPageBuilder.GetEffectivePadding(),
                    contentPageBuilder.GetEffectiveBackgroundColor(),
                    contentPageBuilder.GetContent(),
                    contentPageBuilder.GetPageDefaultFontFamily(),
                    contentPageBuilder.GetPageDefaultFontSize(),
                    contentPageBuilder.GetPageDefaultTextColor(),
                    contentPageBuilder.GetPageDefaultFontAttributes(),
                    contentPageBuilder.GetPageDefaultTextDecorations(),
                    contentPageBuilder.GetPageDefaultTextTransform()
                );
                pageDataList.Add(pageData);
            }
            else
            {
                _logger.LogWarning("Unknown page builder type found: {PageBuilderType}. Skipping page.", pageBuilder.GetType().FullName);
            }
        }

        if (pageDataList.Count == 0)
        {
            throw new InvalidOperationException("Cannot save PDF document: No pages have been added or processed.");
        }

        var meta = _configurationBuilder.MetaDataBuilder;
        var documentData = new PdfDocumentData(
            pageDataList.AsReadOnly(),
            meta.GetTitle, meta.GetAuthor, meta.GetSubject, meta.GetKeywords,
            meta.GetCreator, meta.GetProducer, meta.GetCreationDate,
            meta.GetCustomProperties
        );

        try
        {
            await _pdfGenerationService.GenerateAsync(documentData, path, _configurationBuilder.FontRegistry);
        }
        catch (PdfGenerationException genEx)
        {
            _logger.LogError(genEx, "A known PDF generation error occurred.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while saving the PDF.");
            throw new PdfGenerationException($"An unexpected error occurred while saving the PDF: {ex.Message}", ex);
        }
    }

    private List<PdfElementData> GetAllElements()
    {
        var allElements = new List<PdfElementData>();
        foreach (var page in _pages)
        {
            if (page is PdfContentPageBuilder contentPageBuilder)
            {
                var content = contentPageBuilder.GetContent();
                foreach (var element in content)
                {
                    Traverse(element, allElements);
                }
            }
        }
        return allElements;
    }

    private void Traverse(PdfElementData element, List<PdfElementData> list)
    {
        list.Add(element);
        if (element is PdfLayoutElementData layout)
        {
            foreach (var child in layout.Children)
            {
                Traverse(child, list);
            }
        }
    }
}
