using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

public class PdfHorizontalStackLayout : PdfLayoutElement
{
    private readonly PdfFontRegistryBuilder? _fontRegistry;

    internal PdfHorizontalStackLayout(PdfFontRegistryBuilder? fontRegistry)
    {
        _fontRegistry = fontRegistry;
        GetHorizontalOptions = LayoutAlignment.Start;
        GetVerticalOptions = LayoutAlignment.Start;
    }

    internal PdfHorizontalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfHorizontalStackLayout originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        _fontRegistry = originalStyleSource._fontRegistry;
        Spacing(originalStyleSource.GetSpacing);
        BackgroundColor(originalStyleSource.GetBackgroundColor);
        HorizontalOptions(originalStyleSource.GetHorizontalOptions);
        VerticalOptions(originalStyleSource.GetVerticalOptions);
        Margin(originalStyleSource.GetMargin);
        Padding(originalStyleSource.GetPadding);
    }
}
