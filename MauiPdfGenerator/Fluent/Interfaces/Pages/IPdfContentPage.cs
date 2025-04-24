using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Interfaces.Pages;

public interface IPdfContentPage 
{
    void Content<IPdfVisualElement>(Action<IPdfVisualElement> contentAction);

    IPdfContentPage Padding(double uniformPadding);

    IPdfContentPage Padding(double horizontalPadding, double verticalPadding);

    IPdfContentPage Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding);

    IPdfContentPage BackgroundColor(Color backgroundColor);

    IPdfContentPage Title(string title);
}
