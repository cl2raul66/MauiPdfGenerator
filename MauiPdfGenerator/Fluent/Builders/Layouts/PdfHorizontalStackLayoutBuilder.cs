using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

internal class PdfHorizontalStackLayoutBuilder : IBuildablePdfElement, IPdfHorizontalStackLayout, IPdfLayoutChildHorizontalStackLayout, IPdfGridChildHorizontalStackLayout
{
    private readonly PdfHorizontalStackLayoutData _model;
    private readonly PdfFontRegistryBuilder _fontRegistry;

    public PdfHorizontalStackLayoutBuilder(PdfFontRegistryBuilder fontRegistry)
    {
        _model = new PdfHorizontalStackLayoutData();
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
        elementModel.ApplyContextualDefaults(LayoutAlignment.Start, LayoutAlignment.Fill);
        _model.Add(elementModel);
    }

    #region Public API
    public IPdfGridChildHorizontalStackLayout Spacing(float value) { _model.SetSpacing(value); return this; }

    public IPdfGridChildHorizontalStackLayout Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGridChildHorizontalStackLayout WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    public IPdfGridChildHorizontalStackLayout HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGridChildHorizontalStackLayout BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGridChildHorizontalStackLayout HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGridChildHorizontalStackLayout VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGridChildHorizontalStackLayout Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildHorizontalStackLayout Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildHorizontalStackLayout RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildHorizontalStackLayout ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildHorizontalStackLayout Style(PdfStyleIdentifier key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfHorizontalStackLayout
    IPdfHorizontalStackLayout IPdfHorizontalStackLayout<IPdfHorizontalStackLayout>.Spacing(float v) { Spacing(v); return this; }
    void IPdfHorizontalStackLayout<IPdfHorizontalStackLayout>.Children(Action<IPdfStackLayoutBuilder> s) { Children(s); }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfHorizontalStackLayout IPdfLayoutChild<IPdfHorizontalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfHorizontalStackLayout IPdfLayoutChild<IPdfHorizontalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfLayoutChildHorizontalStackLayout
    IPdfLayoutChildHorizontalStackLayout IPdfHorizontalStackLayout<IPdfLayoutChildHorizontalStackLayout>.Spacing(float v) { Spacing(v); return this; }
    void IPdfHorizontalStackLayout<IPdfLayoutChildHorizontalStackLayout>.Children(Action<IPdfStackLayoutBuilder> s) { Children(s); }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfLayoutChild<IPdfLayoutChildHorizontalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfLayoutChild<IPdfLayoutChildHorizontalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfGridChildHorizontalStackLayout
    IPdfGridChildHorizontalStackLayout IPdfElement<IPdfGridChildHorizontalStackLayout>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    #endregion
}
