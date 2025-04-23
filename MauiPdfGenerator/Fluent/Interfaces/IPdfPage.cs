namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfPage : IPdfVisualElement
{
    IPdfPage Content(Action<IPdfView> content);

    IPdfPage Padding(double uniformPadding);

    IPdfPage Padding(double horizontalPadding, double verticalPadding);

    IPdfPage Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding);
}
