using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Configuration;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfDocument
{
    Task SaveAsync();
    Task SaveAsync(string path);
    IPdfDocument Configuration(Action<IPdfDocumentConfigurator> documentConfigurator);

    IPdfDocument Resources(Action<IPdfResourceBuilder> resourceBuilder);

    IPdfContentPage<TLayout> ContentPage<TLayout>() where TLayout : class;
    IPdfContentPage<IPdfVerticalStackLayout> ContentPage();

    IPdfReportPage ReportPage();
}
