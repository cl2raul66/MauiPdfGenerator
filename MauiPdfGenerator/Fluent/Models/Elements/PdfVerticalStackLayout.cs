using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfVerticalStackLayout : PdfLayoutElement
{
    private readonly PdfFontRegistryBuilder _fontRegistry;

    internal PdfVerticalStackLayout(PdfFontRegistryBuilder fontRegistry)
    {
        _fontRegistry = fontRegistry;
        GetHorizontalOptions = LayoutAlignment.Start;
        GetVerticalOptions = LayoutAlignment.Start;
    }

    internal PdfVerticalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfVerticalStackLayout originalStyleSource)
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
// --- END OF FILE PdfVerticalStackLayout.cs ---