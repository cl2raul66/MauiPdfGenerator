using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfDocument
{
    public Task SaveAsync();
    public Task SaveAsync(string path);

    public void Configuration(Action<IPdfDocumentConfigurator>? configAction);

    public IPdfContentPage Page();
}
