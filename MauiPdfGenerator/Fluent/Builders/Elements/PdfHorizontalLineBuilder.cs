using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Elements;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Styles;
using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Fluent.Builders.Elements;

internal class PdfHorizontalLineBuilder : IBuildablePdfElement, IPdfPageChildHorizontalLine, IPdfLayoutChildHorizontalLine, IPdfGridChildHorizontalLine, IPdfHorizontalLineStyle
{
    private readonly PdfHorizontalLineData _model;

    public PdfHorizontalLineBuilder()
    {
        _model = new PdfHorizontalLineData();
    }

    public PdfElementData GetModel() => _model;

    #region Public API (Implements IPdfGridChildHorizontalLine)
    public IPdfGridChildHorizontalLine Thickness(float value) { if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Thickness must be a positive value."); _model.CurrentThickness = value; return this; }
    public IPdfGridChildHorizontalLine Color(Color color) { _model.CurrentColor = color ?? PdfHorizontalLineData.DefaultColor; return this; }
    public IPdfGridChildHorizontalLine Margin(double uniformMargin) { _model.Margin(uniformMargin); return this; }
    public IPdfGridChildHorizontalLine Margin(double horizontalMargin, double verticalMargin) { _model.Margin(horizontalMargin, verticalMargin); return this; }
    public IPdfGridChildHorizontalLine Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { _model.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalLine Padding(double uniformPadding) { _model.Padding(uniformPadding); return this; }
    public IPdfGridChildHorizontalLine Padding(double horizontalPadding, double verticalPadding) { _model.Padding(horizontalPadding, verticalPadding); return this; }
    public IPdfGridChildHorizontalLine Padding(double leftPadding, double topPadding, double rightMargin, double bottomMargin) { _model.Padding(leftPadding, topPadding, rightMargin, bottomMargin); return this; }
    public IPdfGridChildHorizontalLine WidthRequest(double width) { _model.WidthRequest(width); return this; }
    public IPdfGridChildHorizontalLine HeightRequest(double height) { _model.HeightRequest(height); return this; }
    public IPdfGridChildHorizontalLine BackgroundColor(Color? color) { _model.BackgroundColor(color); return this; }
    public IPdfGridChildHorizontalLine HorizontalOptions(LayoutAlignment layoutAlignment) { _model.HorizontalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalLine VerticalOptions(LayoutAlignment layoutAlignment) { _model.VerticalOptions(layoutAlignment); return this; }
    public IPdfGridChildHorizontalLine Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildHorizontalLine Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildHorizontalLine RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildHorizontalLine ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildHorizontalLine Style(string key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfHorizontalLine<TSelf>
    IPdfPageChildHorizontalLine IPdfHorizontalLine<IPdfPageChildHorizontalLine>.Thickness(float value) { Thickness(value); return this; }
    IPdfLayoutChildHorizontalLine IPdfHorizontalLine<IPdfLayoutChildHorizontalLine>.Thickness(float value) { Thickness(value); return this; }
    IPdfGridChildHorizontalLine IPdfHorizontalLine<IPdfGridChildHorizontalLine>.Thickness(float value) { Thickness(value); return this; }
    IPdfPageChildHorizontalLine IPdfHorizontalLine<IPdfPageChildHorizontalLine>.Color(Color color) { Color(color); return this; }
    IPdfLayoutChildHorizontalLine IPdfHorizontalLine<IPdfLayoutChildHorizontalLine>.Color(Color color) { Color(color); return this; }
    IPdfGridChildHorizontalLine IPdfHorizontalLine<IPdfGridChildHorizontalLine>.Color(Color color) { Color(color); return this; }

    // IPdfElement<TSelf>
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Style(string key) { Style(key); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Style(string key) { Style(key); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChild<TSelf>
    IPdfLayoutChildHorizontalLine IPdfLayoutChild<IPdfLayoutChildHorizontalLine>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildHorizontalLine IPdfLayoutChild<IPdfLayoutChildHorizontalLine>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    IPdfHorizontalLineStyle IPdfHorizontalLine<IPdfHorizontalLineStyle>.Thickness(float value)
    {
        Thickness(value); return this;
    }

    IPdfHorizontalLineStyle IPdfHorizontalLine<IPdfHorizontalLineStyle> .Color(Color color)
    {
        Color(color); return this;
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Margin(double uniformMargin)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Margin(double horizontalMargin, double verticalMargin)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Padding(double uniformPadding)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Padding(double horizontalPadding, double verticalPadding)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.WidthRequest(double width)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.HeightRequest(double height)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.BackgroundColor(Color? color)
    {
        throw new NotImplementedException();
    }

    IPdfHorizontalLineStyle IPdfElement<IPdfHorizontalLineStyle>.Style(string key)
    {
        throw new NotImplementedException();
    }
    #endregion
}
