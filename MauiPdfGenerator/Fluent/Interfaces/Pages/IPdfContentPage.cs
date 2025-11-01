using MauiPdfGenerator.Fluent.Interfaces.Builders;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage<TContent> where TContent : class
{
    IPdfContentPage<TContent> PageSize(Enums.PageSizeType pageSizeType);
    IPdfContentPage<TContent> PageOrientation(Enums.PageOrientationType pageOrientationType);
    IPdfContentPage<TContent> Padding(float uniformPadding);
    IPdfContentPage<TContent> Padding(float verticalPadding, float horizontalPadding);
    IPdfContentPage<TContent> Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding);
    IPdfContentPage<TContent> Padding(Enums.DefaultPagePaddingType defaultPaddingType);
    IPdfContentPage<TContent> BackgroundColor(Color backgroundColor);
    IPdfContentPage<TContent> DefaultFont(Action<IPdfFontDefaultsBuilder> fontDefaults);
    IPdfContentPage<TContent> DefaultTextColor(Color color);
    IPdfContentPage<TContent> DefaultTextDecorations(TextDecorations decorations);
    IPdfContentPage<TContent> DefaultTextTransform(TextTransform transform);

    // El método de transición al contenido del layout raíz.
    IPageReadyToBuild Content(Action<TContent> contentSetup);
}
