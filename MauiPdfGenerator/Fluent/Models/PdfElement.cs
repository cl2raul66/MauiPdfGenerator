using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Fluent.Models;

public abstract class PdfElement : IGridCellInfo
{
    internal PdfElement? Parent { get; set; }
    private PdfLayoutDefaultOptions.DefaultOptions? _defaultOptions;

    protected PdfLayoutDefaultOptions.DefaultOptions DefaultOptions =>
        _defaultOptions ??= PdfLayoutDefaultOptions.GetDefaultOptions(Parent?.GetType() ?? typeof(object), GetType());

    internal Thickness GetMargin { get; private set; }
    internal Thickness GetPadding { get; private set; }
    internal double? GetWidthRequest { get; private set; }
    internal double? GetHeightRequest { get; private set; }
    internal Color? GetBackgroundColor { get; private set; }

    private LayoutAlignment? _horizontalOptions;
    internal LayoutAlignment GetHorizontalOptions => _horizontalOptions ?? DefaultOptions.HorizontalOptions;

    private LayoutAlignment? _verticalOptions;
    internal LayoutAlignment GetVerticalOptions => _verticalOptions ?? DefaultOptions.VerticalOptions;

    internal int GridRow { get; set; } = 0;
    internal int GridColumn { get; set; } = 0;
    internal int GridRowSpan { get; set; } = 1;
    internal int GridColumnSpan { get; set; } = 1;

    public PdfElement Margin(double uniformMargin)
    {
        this.GetMargin = new Thickness(uniformMargin);
        return this;
    }

    public PdfElement Margin(double horizontalMargin, double verticalMargin)
    {
        this.GetMargin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    public PdfElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        this.GetMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public PdfElement Padding(double uniformPadding)
    {
        this.GetPadding = new Thickness(uniformPadding);
        return this;
    }

    public PdfElement Padding(double horizontalPadding, double verticalPadding)
    {
        this.GetPadding = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    public PdfElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding)
    {
        this.GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding);
        return this;
    }

    public PdfElement WidthRequest(double width)
    {
        this.GetWidthRequest = width;
        return this;
    }

    public PdfElement HeightRequest(double height)
    {
        this.GetHeightRequest = height;
        return this;
    }

    public PdfElement BackgroundColor(Color? color)
    {
        this.GetBackgroundColor = color;
        return this;
    }

    public PdfElement HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        this._horizontalOptions = layoutAlignment;
        return this;
    }

    public PdfElement VerticalOptions(LayoutAlignment layoutAlignment)
    {
        this._verticalOptions = layoutAlignment;
        return this;
    }

    int IGridCellInfo.Row => GridRow;
    int IGridCellInfo.Column => GridColumn;
    int IGridCellInfo.RowSpan => GridRowSpan;
    int IGridCellInfo.ColumnSpan => GridColumnSpan;
}
