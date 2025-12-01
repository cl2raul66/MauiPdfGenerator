using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

namespace MauiPdfGenerator.Fluent.Builders.Layouts;

using global::MauiPdfGenerator.Fluent.Interfaces.Styles;

internal class PdfHorizontalStackLayoutBuilder : IBuildablePdfElement, IPdfHorizontalStackLayout, IPdfLayoutChildHorizontalStackLayout, IPdfGridChildHorizontalStackLayout, IPdfHorizontalStackLayoutStyle
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
    public IPdfGridChildHorizontalStackLayout Spacing(float value) { _model.Spacing(value); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildHorizontalStackLayout Padding(double leftPadding, double topPadding, double rightMargin, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalStackLayout WidthRequest(double width) { _model.WidthRequest(width); return this; }
    public IPdfGridChildHorizontalStackLayout HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGridChildHorizontalStackLayout BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGridChildHorizontalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalStackLayout Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildHorizontalStackLayout Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildHorizontalStackLayout RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildHorizontalStackLayout ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }

    // CORRECCIÓN: Implementación real de Style
    public IPdfGridChildHorizontalStackLayout Style(string key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations

    // IPdfHorizontalStackLayoutStyle (NUEVO)
    IPdfHorizontalStackLayoutStyle IPdfHorizontalStackLayout<IPdfHorizontalStackLayoutStyle>.Spacing(float value) { Spacing(value); return this; }
    void IPdfHorizontalStackLayout<IPdfHorizontalStackLayoutStyle>.Children(Action<IPdfStackLayoutBuilder> childrenSetup) { Children(childrenSetup); }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Style(string key) { Style(key); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Margin(double u) { Margin(u); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Padding(double u) { Padding(u); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfHorizontalStackLayoutStyle IPdfElement<IPdfHorizontalStackLayoutStyle>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfHorizontalStackLayout
    IPdfHorizontalStackLayout IPdfHorizontalStackLayout<IPdfHorizontalStackLayout>.Spacing(float value) { Spacing(value); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfHorizontalStackLayout Interfaces.IPdfElement<IPdfHorizontalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfHorizontalStackLayout IPdfLayoutChild<IPdfHorizontalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfHorizontalStackLayout IPdfLayoutChild<IPdfHorizontalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // CORRECCIÓN: Implementación explícita de Style
    IPdfHorizontalStackLayout IPdfElement<IPdfHorizontalStackLayout>.Style(string key) { Style(key); return this; }

    // IPdfLayoutChildHorizontalStackLayout
    IPdfLayoutChildHorizontalStackLayout IPdfHorizontalStackLayout<IPdfLayoutChildHorizontalStackLayout>.Spacing(float value) { Spacing(value); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildHorizontalStackLayout Interfaces.IPdfElement<IPdfLayoutChildHorizontalStackLayout>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfLayoutChild<IPdfLayoutChildHorizontalStackLayout>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildHorizontalStackLayout IPdfLayoutChild<IPdfLayoutChildHorizontalStackLayout>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // CORRECCIÓN: Implementación explícita de Style
    IPdfLayoutChildHorizontalStackLayout IPdfElement<IPdfLayoutChildHorizontalStackLayout>.Style(string key) { Style(key); return this; }

    // IPdfGridChildHorizontalStackLayout
    IPdfGridChildHorizontalStackLayout IPdfElement<IPdfGridChildHorizontalStackLayout>.Style(string key) { Style(key); return this; }
    #endregion
}
