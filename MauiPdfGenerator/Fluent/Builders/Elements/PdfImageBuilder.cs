using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfImageBuilder : IBuildablePdfElement, IPdfPageChildImage, IPdfLayoutChildImage, IPdfGridChildImage
{
    private readonly PdfImageData _model;

    public PdfImageBuilder(Stream stream)
    {
        _model = new PdfImageData(stream);
    }

    public PdfElementData GetModel() => _model;

    #region Public API (Implements IPdfGridChildImage)
    public IPdfGridChildImage Aspect(Aspect aspect) { _model.CurrentAspect = aspect; return this; }
    public IPdfGridChildImage Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGridChildImage Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildImage Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildImage Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGridChildImage Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildImage Padding(double leftPadding, double topPadding, double rightMargin, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightMargin, bottomMargin); return this; }
    public IPdfGridChildImage WidthRequest(double width) { _model.WidthRequest(width); return this; }
    public IPdfGridChildImage HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGridChildImage BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGridChildImage HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildImage VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfGridChildImage Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildImage Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildImage RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildImage ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfImage<TSelf>
    IPdfPageChildImage Interfaces.Elements.IPdfImage<IPdfPageChildImage>.Aspect(Aspect aspect) { Aspect(aspect); return this; }
    IPdfLayoutChildImage Interfaces.Elements.IPdfImage<IPdfLayoutChildImage>.Aspect(Aspect aspect) { Aspect(aspect); return this; }
    IPdfGridChildImage Interfaces.Elements.IPdfImage<IPdfGridChildImage>.Aspect(Aspect aspect) { Aspect(aspect); return this; }

    // IPdfElement<TSelf>
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChild<TSelf>
    IPdfLayoutChildImage IPdfLayoutChild<IPdfLayoutChildImage>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildImage IPdfLayoutChild<IPdfLayoutChildImage>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }
    #endregion
}
