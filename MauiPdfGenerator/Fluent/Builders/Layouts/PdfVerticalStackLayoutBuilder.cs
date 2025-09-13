using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfVerticalStackLayoutBuilder : IPdfVerticalStackLayout, IBuildablePdfElement
{
    private readonly PdfVerticalStackLayoutData _model;

    public PdfVerticalStackLayoutBuilder()
    {
        _model = new PdfVerticalStackLayoutData();
    }

    public PdfElementData GetModel() => _model;

    public void Add(IBuildablePdfElement element)
    {
        _model.Add(element.GetModel());
    }

    public IPdfVerticalStackLayout Spacing(float value) { _model.Spacing(value); return this; }
    public IPdfVerticalStackLayout BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfVerticalStackLayout HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfVerticalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfVerticalStackLayout Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfVerticalStackLayout Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfVerticalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfVerticalStackLayout Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfVerticalStackLayout Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfVerticalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfVerticalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfVerticalStackLayout WidthRequest(double width) { _model.WidthRequest(width); return this; }
}
