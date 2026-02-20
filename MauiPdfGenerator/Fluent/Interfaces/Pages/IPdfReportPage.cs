using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfReportPage : IPdfConfigurablePage<IPdfReportPage>
{
    IPageReadyToBuild Header(Action<IPdfStackLayoutBuilder> headerContent);

    IPageReadyToBuild Content(Action<IPdfStackLayoutBuilder> contentSetup);

    IPageReadyToBuild Footer(Action<IPdfStackLayoutBuilder> footerContent);

    IPdfDocument Build();
}
