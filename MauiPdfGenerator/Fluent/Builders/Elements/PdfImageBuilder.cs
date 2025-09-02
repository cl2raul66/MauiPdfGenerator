using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfImageBuilder : IPdfImage, IBuildableElement
{
    private readonly PdfImageData _model;

    public PdfImageBuilder(Stream stream)
    {
        _model = new PdfImageData(stream);
    }

    public PdfElementData GetModel() => _model;

    public IPdfImage Aspect(Aspect aspect) { _model.CurrentAspect = aspect; return this; }
    public IPdfImage BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfImage HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfImage HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfImage Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfImage Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfImage Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfImage Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfImage Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfImage Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public IPdfImage VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfImage WidthRequest(double width) { _model.WidthRequest(width); return this; }
}
