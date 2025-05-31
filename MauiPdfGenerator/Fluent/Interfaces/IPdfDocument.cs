using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfDocument
{
    public Task SaveAsync();
    public Task SaveAsync(string path);
    IPdfDocument Configuration(Action<IPdfDocumentConfigurator> documentConfigurator);

    IPdfContentPage ContentPage();
}
