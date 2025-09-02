using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfHorizontalStackLayoutBuilder : IPdfHorizontalStackLayout, IBuildableElement
{
    private readonly PdfHorizontalStackLayoutData _model;

    public PdfHorizontalStackLayoutBuilder()
    {
        _model = new PdfHorizontalStackLayoutData();
    }

    public PdfElementData GetModel() => _model;

    public void Add(IBuildableElement element)
    {
        _model.Add(element.GetModel());
    }

    public IPdfHorizontalStackLayout Spacing(float value) { _model.Spacing(value); return this; }
    public IPdfHorizontalStackLayout BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfHorizontalStackLayout HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfHorizontalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfHorizontalStackLayout Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfHorizontalStackLayout Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfHorizontalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfHorizontalStackLayout Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfHorizontalStackLayout Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfHorizontalStackLayout Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfHorizontalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfHorizontalStackLayout WidthRequest(double width) { _model.WidthRequest(width); return this; }
}
