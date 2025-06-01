using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage : IPdfPage<IPdfContentPage>
{
    IPdfContentPage Spacing(float value);
    IPdfContentPage DefaultFont(Action<IFontDefaultsBuilder> fontDefaults);
    IPdfContentPage DefaultTextColor(Color color);
    IPdfContentPage DefaultTextDecorations(TextDecorations decorations);
    IPdfContentPage DefaultTextTransform(TextTransform transform);
    IPageReadyToBuild Content(Action<IPageContentBuilder> contentSetup);
}
