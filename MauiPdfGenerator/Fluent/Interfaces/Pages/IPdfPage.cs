using MauiPdfGenerator.Fluent.Enums;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfPage<TSelf> where TSelf : IPdfPage<TSelf>
{
    TSelf PageSize(PageSizeType pageSizeType);

    TSelf PageOrientation(PageOrientationType pageOrientationType);

    TSelf Padding(float uniformPadding);

    TSelf Padding(float verticalPadding, float horizontalPadding);

    TSelf Padding(float leftPadding, float topPadding, float rightPadding, float bottomPadding);

    TSelf Padding(DefaultPagePaddingType defaultPaddingType);

    TSelf BackgroundColor(Color backgroundColor);

    IPdfDocument Build();
}
