using MauiPdfGenerator.Common;
using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

public class PdfVerticalStackLayout : PdfLayoutElement, ILayoutElement
{
    internal PdfVerticalStackLayout(PdfFontRegistryBuilder? fontRegistry) : base(fontRegistry)
    {
    }

    internal PdfVerticalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfVerticalStackLayout originalStyleSource)
        : base(remainingChildren, originalStyleSource)
    {
    }

    public new PdfVerticalStackLayout Spacing(float value) { base.Spacing(value); return this; }
    public new PdfVerticalStackLayout BackgroundColor(Color? color) { base.BackgroundColor(color); return this; }
    public new PdfVerticalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { base.HorizontalOptions(layoutAlignment); return this; }
    public new PdfVerticalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { base.VerticalOptions(layoutAlignment); return this; }
    public new PdfVerticalStackLayout Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfVerticalStackLayout Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfVerticalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfVerticalStackLayout Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfVerticalStackLayout Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfVerticalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfVerticalStackLayout WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfVerticalStackLayout HeightRequest(double height) { base.HeightRequest(height); return this; }

    IReadOnlyList<object> ILayoutElement.Children => _children.Cast<object>().ToList();
    LayoutType ILayoutElement.LayoutType => LayoutType.VerticalStack;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;
}
