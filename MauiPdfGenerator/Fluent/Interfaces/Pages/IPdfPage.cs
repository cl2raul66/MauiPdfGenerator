using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPage<TSelf> where TSelf : IPdfPage<TSelf>
{
    TSelf PageSize(PageSizeType pageSizeType);

    TSelf PageOrientation(PageOrientationType pageOrientationType);

    TSelf Margins(float uniformMargin);

    TSelf Margins(float verticalMargin, float horizontalMargin);

    TSelf Margins(float leftMargin, float topMargin, float rightMargin, float bottomMargin);

    TSelf Margins(DefaultMarginType defaultMarginType);

    TSelf DefaultFont(string fontAlias);

    TSelf BackgroundColor(Color backgroundColor);

    IPdfDocument Build();
}
