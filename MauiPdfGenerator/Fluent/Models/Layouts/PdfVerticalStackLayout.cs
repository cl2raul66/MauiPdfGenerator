using MauiPdfGenerator.Fluent.Builders;
using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

public class PdfVerticalStackLayout : PdfLayoutElement, ILayoutElement
{
    private readonly PdfFontRegistryBuilder? _fontRegistry;

    internal PdfVerticalStackLayout(PdfFontRegistryBuilder? fontRegistry)
    {
        _fontRegistry = fontRegistry;
    }

    internal PdfVerticalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfVerticalStackLayout originalStyleSource)
    {
        _children.AddRange(remainingChildren);
        _fontRegistry = originalStyleSource._fontRegistry;
        Spacing(originalStyleSource.GetSpacing);
        BackgroundColor(originalStyleSource.GetBackgroundColor);
        HorizontalOptions(originalStyleSource.GetHorizontalOptions);
        VerticalOptions(originalStyleSource.GetVerticalOptions);
        Margin(originalStyleSource.GetMargin.Left, originalStyleSource.GetMargin.Top, originalStyleSource.GetMargin.Right, originalStyleSource.GetMargin.Bottom);
        Padding(originalStyleSource.GetPadding.Left, originalStyleSource.GetPadding.Top, originalStyleSource.GetPadding.Right, originalStyleSource.GetPadding.Bottom);
    }

    // ILayoutElement implementation
    IReadOnlyList<object> ILayoutElement.Children => _children.Cast<object>().ToList();
    LayoutType ILayoutElement.LayoutType => LayoutType.VerticalStack;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;
}