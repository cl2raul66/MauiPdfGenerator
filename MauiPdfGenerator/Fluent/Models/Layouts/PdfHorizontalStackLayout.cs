using MauiPdfGenerator.Common;
using MauiPdfGenerator.Fluent.Builders;

namespace MauiPdfGenerator.Fluent.Models.Layouts;

public class PdfHorizontalStackLayout : PdfLayoutElement, ILayoutElement
{
    internal PdfHorizontalStackLayout(PdfFontRegistryBuilder? fontRegistry) : base(fontRegistry) { }

    internal PdfHorizontalStackLayout(IEnumerable<PdfElement> remainingChildren, PdfHorizontalStackLayout originalStyleSource)
        : base(remainingChildren, originalStyleSource) { }

    public new PdfHorizontalStackLayout Spacing(float value)
    {
        base.Spacing(value);
        return this;
    }
    public new PdfHorizontalStackLayout BackgroundColor(Color? color)
    {
        base.BackgroundColor(color);
        return this;
    }
    public new PdfHorizontalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        base.HorizontalOptions(layoutAlignment);
        return this;
    }
    public new PdfHorizontalStackLayout VerticalOptions(LayoutAlignment layoutAlignment)
    {
        base.VerticalOptions(layoutAlignment);
        return this;
    }
    public new PdfHorizontalStackLayout Margin(double uniformMargin)
    {
        base.Margin(uniformMargin);
        return this;
    }
    public new PdfHorizontalStackLayout Margin(double horizontalMargin, double verticalMargin)
    {
        base.Margin(horizontalMargin, verticalMargin);
        return this;
    }
    public new PdfHorizontalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        base.Margin(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }
    public new PdfHorizontalStackLayout Padding(double uniformPadding)
    {
        base.Padding(uniformPadding);
        return this;
    }
    public new PdfHorizontalStackLayout Padding(double horizontalPadding, double verticalPadding)
    {
        base.Padding(horizontalPadding, verticalPadding);
        return this;
    }
    public new PdfHorizontalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        base.Padding(leftPadding, topPadding, rightPadding, bottomMargin);
        return this;
    }
    public new PdfHorizontalStackLayout WidthRequest(double width)
    {
        base.WidthRequest(width);
        return this;
    }
    public new PdfHorizontalStackLayout HeightRequest(double height)
    {
        base.HeightRequest(height);
        return this;
    }

    IReadOnlyList<object> ILayoutElement.Children => [.. _children.Cast<object>()];
    LayoutType ILayoutElement.LayoutType => LayoutType.HorizontalStack;
    Thickness ILayoutElement.Margin => GetMargin;
    Thickness ILayoutElement.Padding => GetPadding;
}
