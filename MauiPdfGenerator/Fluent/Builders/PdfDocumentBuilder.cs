using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core.Exceptions;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfDocumentBuilder : IPdfDocument
{
    private string? _filePath;
    private readonly PdfConfigurationBuilder _configurationBuilder;
    private readonly List<IPdfPageBuilder> _pages;

    private readonly IPdfGenerationService _pdfGenerationService;

    public PdfDocumentBuilder(string? defaultPath = null)
    {
        _filePath = defaultPath;
        _pages = [];
        _configurationBuilder = new();
        _pdfGenerationService = new SkPdfGenerationService();
    }

    public IPdfDocument Configuration(Action<IPdfDocumentConfigurator> documentConfigurator)
    {
        ArgumentNullException.ThrowIfNull(documentConfigurator);
        documentConfigurator(_configurationBuilder);
        return this;
    }

    public IPdfContentPage ContentPage()
    {
        var pageBuilder = new PdfContentPageBuilder(this, _configurationBuilder);
        _pages.Add(pageBuilder);
        return pageBuilder;
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

        var pageDataList = new List<PdfPageData>();
        foreach (var pageBuilder in _pages)
        {
            if (pageBuilder is IPdfContentPageBuilder contentPageBuilder)
            {
                var pageData = new PdfPageData(
                    contentPageBuilder.GetEffectivePageSize(),
                    contentPageBuilder.GetEffectivePageOrientation(),
                    contentPageBuilder.GetEffectiveMargin(),
                    contentPageBuilder.GetEffectiveBackgroundColor(),
                    contentPageBuilder.GetElements(),
                    contentPageBuilder.GetPageSpacing(),
                    contentPageBuilder.GetPageDefaultFontFamily(),
                    contentPageBuilder.GetPageDefaultFontSize(),
                    contentPageBuilder.GetPageDefaultTextColor(),
                     contentPageBuilder.GetPageDefaultFontAttributes()
                );
                pageDataList.Add(pageData);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Warning: Unknown page builder type encountered: {pageBuilder.GetType().FullName}. Skipping page.");
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
            await _pdfGenerationService.GenerateAsync(documentData, path);
        }
        catch (PdfGenerationException genEx) 
        {
            System.Diagnostics.Debug.WriteLine($"PDF Generation Error: {genEx.Message}");
            throw; 
        }
        catch (Exception ex) 
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error during PDF Save: {ex.Message}");
            throw new PdfGenerationException($"An unexpected error occurred while saving the PDF: {ex.Message}", ex);
        }
    }
}
