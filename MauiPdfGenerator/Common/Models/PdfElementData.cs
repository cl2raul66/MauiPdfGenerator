using MauiPdfGenerator.Common.Enums;
using MauiPdfGenerator.Common.Models.Styling;
using MauiPdfGenerator.Fluent.Models;

namespace MauiPdfGenerator.Common.Models;

internal abstract class PdfElementData : IPdfGridCellInfo
{
    internal PdfElementData? Parent { get; set; }

    internal PdfStyleIdentifier? StyleKey { get; private set; }

    internal PdfStyledProperty<Thickness> MarginProp { get; } = new(Thickness.Zero);
    internal PdfStyledProperty<Thickness> PaddingProp { get; } = new(Thickness.Zero);
    internal PdfStyledProperty<double?> WidthRequestProp { get; } = new(null);
    internal PdfStyledProperty<double?> HeightRequestProp { get; } = new(null);
    internal PdfStyledProperty<Color?> BackgroundColorProp { get; } = new(null);
    internal PdfStyledProperty<LayoutAlignment> HorizontalOptionsProp { get; } = new(LayoutAlignment.Fill);
    internal PdfStyledProperty<LayoutAlignment> VerticalOptionsProp { get; } = new(LayoutAlignment.Start);

    internal Thickness GetMargin => MarginProp.Value;
    internal Thickness GetPadding => PaddingProp.Value;
    internal double? GetWidthRequest => WidthRequestProp.Value;
    internal double? GetHeightRequest => HeightRequestProp.Value;
    internal Color? GetBackgroundColor => BackgroundColorProp.Value;
    internal LayoutAlignment GetHorizontalOptions => HorizontalOptionsProp.Value;
    internal LayoutAlignment GetVerticalOptions => VerticalOptionsProp.Value;

    internal bool HorizontalOptionsSet => HorizontalOptionsProp.Priority > PdfPropertyPriority.Default;
    internal bool VerticalOptionsSet => VerticalOptionsProp.Priority > PdfPropertyPriority.Default;

    internal int GridRow { get; private set; } = 0;
    internal int GridColumn { get; private set; } = 0;
    internal int GridRowSpan { get; private set; } = 1;
    internal int GridColumnSpan { get; private set; } = 1;

    protected PdfElementData() { }

    public PdfElementData Style(PdfStyleIdentifier key)
    {
        if (string.IsNullOrWhiteSpace(key.Key)) throw new ArgumentException("Style key cannot be empty.", nameof(key));
        this.StyleKey = key;
        return this;
    }

    internal void ApplyContextualDefaults(LayoutAlignment horizontal, LayoutAlignment vertical)
    {
        if (HorizontalOptionsProp.Priority is PdfPropertyPriority.Default)
        {
            HorizontalOptionsProp.Set(horizontal, PdfPropertyPriority.Default);
        }
        if (VerticalOptionsProp.Priority is PdfPropertyPriority.Default)
        {
            VerticalOptionsProp.Set(vertical, PdfPropertyPriority.Default);
        }
    }

    public PdfElementData SetMargin(double u) { MarginProp.Set(new Thickness(u), PdfPropertyPriority.Local); return this; }
    public PdfElementData SetMargin(double h, double v) { MarginProp.Set(new Thickness(h, v), PdfPropertyPriority.Local); return this; }
    public PdfElementData SetMargin(double l, double t, double r, double b) { MarginProp.Set(new Thickness(l, t, r, b), PdfPropertyPriority.Local); return this; }

    public PdfElementData SetPadding(double u) { PaddingProp.Set(new Thickness(u), PdfPropertyPriority.Local); return this; }
    public PdfElementData SetPadding(double h, double v) { PaddingProp.Set(new Thickness(h, v), PdfPropertyPriority.Local); return this; }
    public PdfElementData SetPadding(double l, double t, double r, double b) { PaddingProp.Set(new Thickness(l, t, r, b), PdfPropertyPriority.Local); return this; }

    public PdfElementData SetWidthRequest(double w) { WidthRequestProp.Set(w, PdfPropertyPriority.Local); return this; }
    public PdfElementData SetHeightRequest(double h) { HeightRequestProp.Set(h, PdfPropertyPriority.Local); return this; }
    public PdfElementData SetBackgroundColor(Color? c) { BackgroundColorProp.Set(c, PdfPropertyPriority.Local); return this; }
    public PdfElementData SetHorizontalOptions(LayoutAlignment a) { HorizontalOptionsProp.Set(a, PdfPropertyPriority.Local); return this; }
    public PdfElementData SetVerticalOptions(LayoutAlignment a) { VerticalOptionsProp.Set(a, PdfPropertyPriority.Local); return this; }

    internal void SetRow(int row) { if (row >= 0) this.GridRow = row; else throw new ArgumentOutOfRangeException(nameof(row)); }
    internal void SetColumn(int column) { if (column >= 0) this.GridColumn = column; else throw new ArgumentOutOfRangeException(nameof(column)); }
    internal void SetRowSpan(int span) { if (span >= 1) this.GridRowSpan = span; else throw new ArgumentOutOfRangeException(nameof(span)); }
    internal void SetColumnSpan(int span) { if (span >= 1) this.GridColumnSpan = span; else throw new ArgumentOutOfRangeException(nameof(span)); }

    int IPdfGridCellInfo.Row => GridRow;
    int IPdfGridCellInfo.Column => GridColumn;
    int IPdfGridCellInfo.RowSpan => GridRowSpan;
    int IPdfGridCellInfo.ColumnSpan => GridColumnSpan;
}
