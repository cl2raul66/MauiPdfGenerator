using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders;

internal class PdfDocumentBuilder : IPdfDocument
{
    private string? _filePath;
    readonly PdfConfigurationBuilder _configurationBuilder;
    readonly List<IPdfPageBuilder> _pages;

    public PdfDocumentBuilder(string? path = null)
    {
        _filePath = path;
        _pages = [];
        _configurationBuilder = new();
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

    public async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            throw new InvalidOperationException("No default file path was specified during document creation. Use SaveAsync(path) or create the document with a path.");
        }

        await SaveAsync(_filePath);
    }

    public async Task SaveAsync(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(nameof(path), "File path cannot be null or empty");
        }



        await Task.CompletedTask; 
    }
}
