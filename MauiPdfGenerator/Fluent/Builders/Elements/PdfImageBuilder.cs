using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfImageBuilder : IBuildablePdfElement, IPdfPageChildImage, IPdfLayoutChildImage, IPdfGridChildImage, IPdfImage
{
    private readonly PdfImageData _model;

    public PdfImageBuilder(Stream stream)
    {
        _model = new PdfImageData(stream);
    }

    public PdfElementData GetModel() => _model;

    #region Public API
    public IPdfGridChildImage Aspect(Aspect aspect) { _model.AspectProp.Set(aspect, PdfPropertyPriority.Local); return this; }

    public IPdfGridChildImage Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGridChildImage Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGridChildImage Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGridChildImage Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGridChildImage Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGridChildImage Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGridChildImage WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    public IPdfGridChildImage HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGridChildImage BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGridChildImage HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGridChildImage VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGridChildImage Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildImage Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildImage RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildImage ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildImage Style(PdfStyleIdentifier key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfImage
    IPdfImage IPdfImage<IPdfImage>.Aspect(Aspect a) { Aspect(a); return this; }
    IPdfImage IPdfElement<IPdfImage>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfImage IPdfElement<IPdfImage>.Margin(double u) { Margin(u); return this; }
    IPdfImage IPdfElement<IPdfImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfImage IPdfElement<IPdfImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfImage IPdfElement<IPdfImage>.Padding(double u) { Padding(u); return this; }
    IPdfImage IPdfElement<IPdfImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfImage IPdfElement<IPdfImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfImage IPdfElement<IPdfImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfImage IPdfElement<IPdfImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfImage IPdfElement<IPdfImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfPageChildImage
    IPdfPageChildImage Interfaces.Elements.IPdfImage<IPdfPageChildImage>.Aspect(Aspect a) { Aspect(a); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildImage IPdfElement<IPdfPageChildImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChildImage
    IPdfLayoutChildImage Interfaces.Elements.IPdfImage<IPdfLayoutChildImage>.Aspect(Aspect a) { Aspect(a); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildImage IPdfElement<IPdfLayoutChildImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildImage IPdfLayoutChild<IPdfLayoutChildImage>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildImage IPdfLayoutChild<IPdfLayoutChildImage>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfGridChildImage
    IPdfGridChildImage Interfaces.Elements.IPdfImage<IPdfGridChildImage>.Aspect(Aspect a) { Aspect(a); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double u) { Margin(u); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double u) { Padding(u); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfGridChildImage IPdfElement<IPdfGridChildImage>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfGridChildImage IPdfLayoutChild<IPdfGridChildImage>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfGridChildImage IPdfLayoutChild<IPdfGridChildImage>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }
    #endregion
}
