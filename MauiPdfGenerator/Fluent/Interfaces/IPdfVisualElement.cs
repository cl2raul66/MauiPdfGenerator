namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfVisualElement 
{
    IPdfVisualElement BackgroundColor(Color backgroundColor);    


    IPdfVisualElement WidthRequest(double widthRequest);

    IPdfVisualElement MinimumWidthRequest(double minimumWidthRequest);

    IPdfVisualElement MaximumWidthRequest(double maximumWidthRequest);

    IPdfVisualElement HeightRequest(double heightRequest);

    IPdfVisualElement MinimumHeightRequest(double minimumHeightRequest);

    IPdfVisualElement MaximumHeightRequest(double maximumHeightRequest);


    IPdfVisualElement Margin(double uniformMargin);

    IPdfVisualElement Margin(double horizontalMargin, double verticalMargin);

    IPdfVisualElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin);

    IPdfVisualElement Padding(double uniformPadding);

    IPdfVisualElement Padding(double horizontalPadding, double verticalPadding);

    IPdfVisualElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding);
}
