
using MauiPdfGenerator.Fluent.Interfaces;

namespace MauiPdfGenerator.Fluent.Models;

public class PdfDocument : IPdfDocument
{
    private string? _filePath;

    public PdfDocument(string? path = null)
    {
        _filePath = path;
    }

    public void Configuration(Action<IPdfDocumentConfigurator>? configAction = null)
    {
        throw new NotImplementedException();
    }

    public void Page(Action<Interfaces.IPdfPage> page)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAsync()
    {
        if (string.IsNullOrEmpty(_filePath))
        {
            throw new InvalidOperationException("No file path specified. Use SaveAsync(path) instead.");
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
