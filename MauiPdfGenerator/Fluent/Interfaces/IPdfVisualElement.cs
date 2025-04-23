namespace MauiPdfGenerator.Fluent.Interfaces;

public interface IPdfVisualElement
{
    IPdfVisualElement BackgroundColor(Color backgroundColor);

    IPdfVisualElement HeightRequest(double heightRequest);

    IPdfVisualElement MinimumHeightRequest(double minimumHeightRequest);

    IPdfVisualElement MaximumHeightRequest(double maximumHeightRequest);

    IPdfVisualElement WidthRequest(double widthRequest);

    IPdfVisualElement Parent();

    IPdfVisualElement MinimumWidthRequest(double minimumWidthRequest);

    IPdfVisualElement MaximumWidthRequest(double maximumWidthRequest);
}
