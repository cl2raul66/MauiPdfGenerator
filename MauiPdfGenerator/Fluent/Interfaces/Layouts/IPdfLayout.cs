using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Interfaces.Layouts;

public interface IPdfLayout : IPdfVisualElement
{
    IPdfVerticalStack Spacing(double spacing);

    IPdfVisualElement HorizontalOptions(LayoutOptions layoutOptions);

    IPdfVisualElement VerticalOptions(LayoutOptions layoutOptions);
}
