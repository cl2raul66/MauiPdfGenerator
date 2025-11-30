namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfElement<TSelf> where TSelf : IPdfElement<TSelf>
{
    TSelf Margin(double uniformMargin);
    TSelf Margin(double horizontalMargin, double verticalMargin);
    TSelf Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin);
    TSelf Padding(double uniformPadding);
    TSelf Padding(double horizontalPadding, double verticalPadding);
    TSelf Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin);
    TSelf WidthRequest(double width);
    TSelf HeightRequest(double height);
    TSelf BackgroundColor(Color? color);
    TSelf Style(string key);
}
