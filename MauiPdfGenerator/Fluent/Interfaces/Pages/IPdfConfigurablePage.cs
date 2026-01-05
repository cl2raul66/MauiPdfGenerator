using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfConfigurablePage<TContent> where TContent : class
{
    IPdfConfigurablePage<TContent> PageSize(PageSizeType pageSizeType);
    IPdfConfigurablePage<TContent> PageOrientation(PageOrientationType pageOrientationType);
    IPdfConfigurablePage<TContent> Padding(float uniformPadding);
    IPdfConfigurablePage<TContent> Padding(float verticalPadding, float horizontalPadding);
    IPdfConfigurablePage<TContent> Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding);
    IPdfConfigurablePage<TContent> Padding(DefaultPagePaddingType defaultPaddingType);
    IPdfConfigurablePage<TContent> BackgroundColor(Color backgroundColor);
    IPdfConfigurablePage<TContent> DefaultFont(Action<IPdfFontDefaultsBuilder> fontDefaults);
    IPdfConfigurablePage<TContent> DefaultTextColor(Color color);
    IPdfConfigurablePage<TContent> DefaultTextDecorations(TextDecorations decorations);
    IPdfConfigurablePage<TContent> DefaultTextTransform(TextTransform transform);
    IPdfConfigurablePage<TContent> Resources(Action<IPdfResourceBuilder> resourceBuilderAction);
    IPdfConfigurablePage<TContent> Culture(string cultureName);
    IPageReadyToBuild Content(Action<TContent> contentSetup);
}
