using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models;
using MauiPdfGenerator.Common.Models.Views;
using MauiPdfGenerator.Fluent.Interfaces;
using MauiPdfGenerator.Fluent.Interfaces.Builders;
using MauiPdfGenerator.Fluent.Interfaces.Views;
using MauiPdfGenerator.Fluent.Interfaces.Layouts;
using MauiPdfGenerator.Fluent.Interfaces.Layouts.Grids;
using MauiPdfGenerator.Fluent.Interfaces.Pages;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Fluent.Builders.Views;

internal class PdfHorizontalLineBuilder : IBuildablePdfElement, IPdfPageChildHorizontalLine, IPdfLayoutChildHorizontalLine, IPdfGridChildHorizontalLine, IPdfHorizontalLine
{
    private readonly PdfHorizontalLineData _model;

    public PdfHorizontalLineBuilder()
    {
        _model = new PdfHorizontalLineData();
    }

    public PdfElementData GetModel() => _model;

    #region Public API
    public IPdfGridChildHorizontalLine Thickness(float value) { if (value > 0) { _model.ThicknessProp.Set(value, PdfPropertyPriority.Local); return this; } throw new ArgumentOutOfRangeException(nameof(value)); }
    public IPdfGridChildHorizontalLine Color(Color color) { _model.ColorProp.Set(color ?? PdfHorizontalLineData.DefaultColor, PdfPropertyPriority.Local); return this; }

    public IPdfGridChildHorizontalLine Margin(double u) { _model.SetMargin(u); return this; }
    public IPdfGridChildHorizontalLine Margin(double h, double v) { _model.SetMargin(h, v); return this; }
    public IPdfGridChildHorizontalLine Margin(double l, double t, double r, double b) { _model.SetMargin(l, t, r, b); return this; }
    public IPdfGridChildHorizontalLine Padding(double u) { _model.SetPadding(u); return this; }
    public IPdfGridChildHorizontalLine Padding(double h, double v) { _model.SetPadding(h, v); return this; }
    public IPdfGridChildHorizontalLine Padding(double l, double t, double r, double b) { _model.SetPadding(l, t, r, b); return this; }
    public IPdfGridChildHorizontalLine WidthRequest(double w) { _model.SetWidthRequest(w); return this; }
    public IPdfGridChildHorizontalLine HeightRequest(double h) { _model.SetHeightRequest(h); return this; }
    public IPdfGridChildHorizontalLine BackgroundColor(Color? c) { _model.SetBackgroundColor(c); return this; }
    public IPdfGridChildHorizontalLine HorizontalOptions(LayoutAlignment a) { _model.SetHorizontalOptions(a); return this; }
    public IPdfGridChildHorizontalLine VerticalOptions(LayoutAlignment a) { _model.SetVerticalOptions(a); return this; }
    public IPdfGridChildHorizontalLine Row(int row) { _model.SetRow(row); return this; }
    public IPdfGridChildHorizontalLine Column(int column) { _model.SetColumn(column); return this; }
    public IPdfGridChildHorizontalLine RowSpan(int span) { _model.SetRowSpan(span); return this; }
    public IPdfGridChildHorizontalLine ColumnSpan(int span) { _model.SetColumnSpan(span); return this; }
    public IPdfGridChildHorizontalLine Style(PdfStyleIdentifier key) { _model.Style(key); return this; }
    #endregion

    #region Explicit Interface Implementations
    // IPdfHorizontalLine
    IPdfHorizontalLine IPdfHorizontalLine<IPdfHorizontalLine>.Thickness(float v) { Thickness(v); return this; }
    IPdfHorizontalLine IPdfHorizontalLine<IPdfHorizontalLine>.Color(Color c) { Color(c); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfHorizontalLine IPdfElement<IPdfHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfPageChildHorizontalLine
    IPdfPageChildHorizontalLine IPdfHorizontalLine<IPdfPageChildHorizontalLine>.Thickness(float v) { Thickness(v); return this; }
    IPdfPageChildHorizontalLine IPdfHorizontalLine<IPdfPageChildHorizontalLine>.Color(Color c) { Color(c); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfPageChildHorizontalLine IPdfElement<IPdfPageChildHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }

    // IPdfLayoutChildHorizontalLine
    IPdfLayoutChildHorizontalLine IPdfHorizontalLine<IPdfLayoutChildHorizontalLine>.Thickness(float v) { Thickness(v); return this; }
    IPdfLayoutChildHorizontalLine IPdfHorizontalLine<IPdfLayoutChildHorizontalLine>.Color(Color c) { Color(c); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfLayoutChildHorizontalLine IPdfElement<IPdfLayoutChildHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfLayoutChildHorizontalLine IPdfLayoutChild<IPdfLayoutChildHorizontalLine>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfLayoutChildHorizontalLine IPdfLayoutChild<IPdfLayoutChildHorizontalLine>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }

    // IPdfGridChildHorizontalLine
    IPdfGridChildHorizontalLine IPdfHorizontalLine<IPdfGridChildHorizontalLine>.Thickness(float v) { Thickness(v); return this; }
    IPdfGridChildHorizontalLine IPdfHorizontalLine<IPdfGridChildHorizontalLine>.Color(Color c) { Color(c); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Style(PdfStyleIdentifier k) { Style(k); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Margin(double u) { Margin(u); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Margin(double h, double v) { Margin(h, v); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Margin(double l, double t, double r, double b) { Margin(l, t, r, b); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Padding(double u) { Padding(u); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Padding(double h, double v) { Padding(h, v); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.Padding(double l, double t, double r, double b) { Padding(l, t, r, b); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.WidthRequest(double w) { WidthRequest(w); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.HeightRequest(double h) { HeightRequest(h); return this; }
    IPdfGridChildHorizontalLine IPdfElement<IPdfGridChildHorizontalLine>.BackgroundColor(Color? c) { BackgroundColor(c); return this; }
    IPdfGridChildHorizontalLine IPdfLayoutChild<IPdfGridChildHorizontalLine>.HorizontalOptions(LayoutAlignment a) { HorizontalOptions(a); return this; }
    IPdfGridChildHorizontalLine IPdfLayoutChild<IPdfGridChildHorizontalLine>.VerticalOptions(LayoutAlignment a) { VerticalOptions(a); return this; }
    #endregion
}
