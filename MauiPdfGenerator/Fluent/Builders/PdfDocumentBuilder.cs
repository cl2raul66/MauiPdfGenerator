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
    // El PdfFontRegistryBuilder ya no se necesita aquí directamente,
    // está dentro de _configurationBuilder

    public PdfDocumentBuilder(PdfFontRegistryBuilder fontRegistry, string? defaultPath = null)
    {
        _filePath = defaultPath;
        _pages = [];
        // PdfConfigurationBuilder recibe y gestiona el PdfFontRegistryBuilder
        _configurationBuilder = new PdfConfigurationBuilder(fontRegistry);
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
                    contentPageBuilder.GetPageDefaultFontFamily(), // Esto es PdfFontIdentifier
                    contentPageBuilder.GetPageDefaultFontSize(),
                    contentPageBuilder.GetPageDefaultTextColor(),
                    contentPageBuilder.GetPageDefaultFontAttributes()
                );
                pageDataList.Add(pageData);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"Advertencia: Tipo de constructor de página desconocido encontrado: {pageBuilder.GetType().FullName}. Omitiendo página.");
            }
        }

        if (pageDataList.Count == 0)
        {
            throw new InvalidOperationException("No se puede guardar el documento PDF: No se han agregado ni procesado páginas.");
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
            // _configurationBuilder.FontRegistry es el PdfFontRegistryBuilder que contiene toda la configuración
            await _pdfGenerationService.GenerateAsync(documentData, path, _configurationBuilder.FontRegistry);
        }
        catch (PdfGenerationException genEx)
        {
            System.Diagnostics.Debug.WriteLine($"Error de generación de PDF: {genEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error inesperado durante el guardado del PDF: {ex.Message}");
            throw new PdfGenerationException($"Ocurrió un error inesperado al guardar el PDF: {ex.Message}", ex);
        }
    }
}
