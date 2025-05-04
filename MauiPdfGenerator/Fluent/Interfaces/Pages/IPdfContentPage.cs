using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage : IPdfPage<IPdfContentPage>
{
    IPdfContentPage Spacing(float value);

    IPdfContentPage DefaultFont(Action<IFontDefaultsBuilder> fontDefaults);

    IPdfContentPage DefaultTextColor(Color color);

    IPageReadyToBuild Content(Action<IPageContentBuilder> contentSetup);
}
