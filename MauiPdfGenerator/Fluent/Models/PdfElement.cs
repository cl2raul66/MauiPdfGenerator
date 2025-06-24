using MauiPdfGenerator.Common;

namespace MauiPdfGenerator.Fluent.Models;

public abstract class PdfElement : IGridCellInfo
{
    internal PdfElement? Parent { get; set; }
    private PdfLayoutDefaultOptions.DefaultOptions? _defaultOptions;

    protected PdfLayoutDefaultOptions.DefaultOptions DefaultOptions => 
        _defaultOptions ??= PdfLayoutDefaultOptions.GetDefaultOptions(Parent?.GetType() ?? typeof(object), GetType());

    internal virtual Thickness GetMargin { get; private set; }
    internal virtual Thickness GetPadding { get; private set; }
    internal virtual double? GetWidthRequest { get; private set; }
    internal virtual double? GetHeightRequest { get; private set; }
    internal virtual Color? GetBackgroundColor { get; private set; }
    internal virtual LayoutAlignment GetHorizontalOptions => _horizontalOptions ?? DefaultOptions.HorizontalOptions;
    internal virtual LayoutAlignment GetVerticalOptions => _verticalOptions ?? DefaultOptions.VerticalOptions;

    public int GridRow { get; private set; } = 0;
    public int GridColumn { get; private set; } = 0;
    public int GridRowSpan { get; private set; } = 1;
    public int GridColumnSpan { get; private set; } = 1;

    private LayoutAlignment? _horizontalOptions;
    private LayoutAlignment? _verticalOptions;

    public PdfElement Margin(double uniformMargin)
    {
        GetMargin = new Thickness(uniformMargin);
        return this;
    }

    public PdfElement Margin(double horizontalMargin, double verticalMargin)
    {
        GetMargin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    public PdfElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        GetMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public PdfElement Padding(double uniformPadding)
    {
        GetPadding = new Thickness(uniformPadding);
        return this;
    }

    public PdfElement Padding(double horizontalPadding, double verticalPadding)
    {
        GetPadding = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    public PdfElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomPadding)
    {
        GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomPadding);
        return this;
    }

    public PdfElement WidthRequest(double width)
    {
        GetWidthRequest = width;
        return this;
    }

    public PdfElement HeightRequest(double height)
    {
        GetHeightRequest = height;
        return this;
    }

    public PdfElement BackgroundColor(Color? color)
    {
        GetBackgroundColor = color;
        return this;
    }

    public PdfElement HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        _horizontalOptions = layoutAlignment;
        return this;
    }

    public PdfElement VerticalOptions(LayoutAlignment layoutAlignment)
    {
        _verticalOptions = layoutAlignment;
        return this;
    }

    public PdfElement Row(int row)
    {
        GridRow = row;
        return this;
    }

    public PdfElement Column(int column)
    {
        GridColumn = column;
        return this;
    }

    public PdfElement RowSpan(int span)
    {
        GridRowSpan = span;
        return this;
    }

    public PdfElement ColumnSpan(int span)
    {
        GridColumnSpan = span;
        return this;
    }

    // IGridCellInfo implementation
    int IGridCellInfo.Row => GridRow;
    int IGridCellInfo.Column => GridColumn;
    int IGridCellInfo.RowSpan => GridRowSpan;
    int IGridCellInfo.ColumnSpan => GridColumnSpan;
}
