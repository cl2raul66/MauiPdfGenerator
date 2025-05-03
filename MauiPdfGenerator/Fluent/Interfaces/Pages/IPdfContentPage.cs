namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage : IPdfPage<IPdfContentPage>
{
    IPdfContentPage Content(Action<IPageContentBuilder> contentSetup);

    IPdfContentPage Spacing(float value);

    IPdfContentPage DefaultFontFamily(string familyName);

    IPdfContentPage DefaultFontSize(float size);

    IPdfContentPage DefaultTextColor(Color color);
}
