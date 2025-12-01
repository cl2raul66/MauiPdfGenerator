using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Layouts;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;

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

    #region Public API (Implements the most specific interface: IPdfGridChildVerticalStackLayout)
    public IPdfGridChildVerticalStackLayout Spacing(float value) { _model.Spacing(value); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildVerticalStackLayout Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildVerticalStackLayout Padding(double leftPadding, double topPadding, double rightMargin, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightMargin, bottomMargin); return this; }
    public IPdfGridChildVerticalStackLayout WidthRequest(double width) { _model.WidthRequest(width); return this; }
    public IPdfGridChildVerticalStackLayout HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGridChildVerticalStackLayout BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGridChildVerticalStackLayout HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildVerticalStackLayout VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfGridChildVerticalStackLayout Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildVerticalStackLayout Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildVerticalStackLayout RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildVerticalStackLayout ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfVerticalStackLayout
    IPdfVerticalStackLayout IPdfVerticalStackLayout<IPdfVerticalStackLayout>.Spacing(float value) { Spacing(value); return this; }
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
    IPdfLayoutChildVerticalStackLayout IPdfVerticalStackLayout<IPdfLayoutChildVerticalStackLayout>.Spacing(float value) { Spacing(value); return this; }
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

    public IPdfVerticalStackLayout Style(string key)
    {
        throw new NotImplementedException();
    }

    IPdfLayoutChildVerticalStackLayout IPdfElement<IPdfLayoutChildVerticalStackLayout>.Style(string key)
    {
        throw new NotImplementedException();
    }

    IPdfGridChildVerticalStackLayout IPdfElement<IPdfGridChildVerticalStackLayout>.Style(string key)
    {
        throw new NotImplementedException();
    }
    #endregion
}
