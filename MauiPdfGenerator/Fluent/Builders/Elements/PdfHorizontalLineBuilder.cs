using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfHorizontalLineBuilder : IPdfHorizontalLine, IBuildableElement
{
    private readonly PdfHorizontalLineData _model;

    public PdfHorizontalLineBuilder()
    {
        _model = new PdfHorizontalLineData();
    }

    public PdfElementData GetModel() => _model;

    public IPdfHorizontalLine BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfHorizontalLine Color(Color color) { _model.CurrentColor = color ?? PdfHorizontalLineData.DefaultColor; return this; }
    public IPdfHorizontalLine HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfHorizontalLine HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfHorizontalLine Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfHorizontalLine Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfHorizontalLine Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfHorizontalLine Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfHorizontalLine Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfHorizontalLine Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfHorizontalLine Thickness(float value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), "Thickness must be a positive value.");
        _model.CurrentThickness = value;
        return this;
    }
    public IPdfHorizontalLine VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfHorizontalLine WidthRequest(double width) { _model.WidthRequest(width); return this; }
}
