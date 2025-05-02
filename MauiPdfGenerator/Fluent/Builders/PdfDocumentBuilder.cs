using MauiPdfGenerator.Core;
using MauiPdfGenerator.Core.Implementation.Sk;
using MauiPdfGenerator.Core.Models;
using MauiPdfGenerator.Fluent.Interfaces;
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
            throw new InvalidOperationException("No se especificó una ruta de archivo por defecto durante la creación del documento. Utilice SaveAsync(path) o cree el documento proporcionando una ruta.");
        }
        return SaveAsync(_filePath);
    }

    public async Task SaveAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "La ruta del archivo no puede ser nula o vacía.");
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
                    contentPageBuilder.GetEffectiveDefaultFontAlias()
                );
                pageDataList.Add(pageData);
            }
        }

        var meta = _configurationBuilder.MetaDataBuilder;
        var documentData = new PdfDocumentData(
            pageDataList.AsReadOnly(),
            meta.GetTitle, meta.GetAuthor, meta.GetSubject, meta.GetKeywords,
            meta.GetCreator, meta.GetProducer, meta.GetCreationDate,
            meta.GetCustomProperties
        );

        // 2. Delegar a Core
        try
        {
            await _pdfGenerationService.GenerateAsync(documentData, path);
        }
        catch (Exception ex) { /*...*/ throw; }
    }

    // --- YA NO SE NECESITAN MÉTODOS DE MAPEO ---
}
