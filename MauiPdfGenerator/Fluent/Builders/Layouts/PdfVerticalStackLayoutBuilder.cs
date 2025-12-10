using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfVerticalStackLayoutBuilder : IBuildablePdfElement, IPdfVerticalStackLayout, IPdfLayoutChildVerticalStackLayout, IPdfGridChildVerticalStackLayout
{
    private readonly PdfVerticalStackLayoutData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfVerticalStackLayoutBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfVerticalStackLayoutData();
        _fontRegistry = fontRegistry;
    }

    public PdfElementData GetModel() => _model;

    public void Children(Action<IPdfStackLayoutBuilder> childrenSetup)
    {
        var childrenBuilder = new PdfStackLayoutContentBuilder(this, _fontRegistry);
        childrenSetup(childrenBuilder);
    }

    internal void Add(IBuildablePdfElement element)
    {
        var elementModel = element.GetModel();
        elementModel.ApplyContextualDefaults(LayoutAlignment.Fill, LayoutAlignment.Start);
        _model.Add(elementModel);
    }

    #region Public API
    public IPdfGridChildVerticalStackLayout Spacing(float value) { _model.SetSpacing(value); return this; }

    public IPdfGridChildVerticalStackLayout Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGridChildVerticalStackLayout WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    public IPdfGridChildVerticalStackLayout HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGridChildVerticalStackLayout BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGridChildVerticalStackLayout HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGridChildVerticalStackLayout VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGridChildVerticalStackLayout Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildVerticalStackLayout Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildVerticalStackLayout RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildVerticalStackLayout ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildVerticalStackLayout Style(PdfStyleIdentifier key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfVerticalStackLayout
    IPdfVerticalStackLayout IPdfVerticalStackLayout<IPdfVerticalStackLayout>.Spacing(float v) { Spacing(v); return this; }
    void IPdfVerticalStackLayout<IPdfVerticalStackLayout>.Children(Action<IPdfStackLayoutBuilder> s) { Children(s); }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfVerticalStackLayout IPdfElement<IPdfVerticalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfVerticalStackLayout IPdfLayoutChild<IPdfVerticalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfVerticalStackLayout IPdfLayoutChild<IPdfVerticalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfLayoutChildVerticalStackLayout
    IPdfLayoutChildVerticalStackLayout IPdfVerticalStackLayout<IPdfLayoutChildVerticalStackLayout>.Spacing(float v) { Spacing(v); return this; }
    void IPdfVerticalStackLayout<IPdfLayoutChildVerticalStackLayout>.Children(Action<IPdfStackLayoutBuilder> s) { Children(s); }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfLayoutChild<IPdfLayoutChildVerticalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildVerticalStackLayout IPdfLayoutChild<IPdfLayoutChildVerticalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfGridChildVerticalStackLayout
    IPdfGridChildVerticalStackLayout IPdfElement<IPdfGridChildVerticalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    #endregion
}
