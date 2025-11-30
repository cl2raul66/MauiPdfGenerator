namespace MauiPdfGenerator.Common.Models;

internal abstract class PdfElementData : IPdfGridCellInfo
{
    private bool _horizontalOptionsSet = false;
    private bool _verticalOptionsSet = false;

    internal PdfElementData? Parent { get; set; }

    internal Thickness GetMargin { get; private set; }
    internal Thickness GetPadding { get; private set; }
    internal double? GetWidthRequest { get; private set; }
    internal double? GetHeightRequest { get; private set; }
    internal Color? GetBackgroundColor { get; private set; }
    public string? StyleKey { get; private set; }

    internal LayoutAlignment GetHorizontalOptions { get; private set; } = LayoutAlignment.Fill;
    internal LayoutAlignment GetVerticalOptions { get; private set; } = LayoutAlignment.Start;

    internal int GridRow { get; private set; } = 0;
    internal int GridColumn { get; private set; } = 0;
    internal int GridRowSpan { get; private set; } = 1;
    internal int GridColumnSpan { get; private set; } = 1;

    public PdfElementData Margin(double uniformMargin)
    {
        this.GetMargin = new Thickness(uniformMargin);
        return this;
    }

    public PdfElementData Margin(double horizontalMargin, double verticalMargin)
    {
        this.GetMargin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    public PdfElementData Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        this.GetMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public PdfElementData Padding(double uniformPadding)
    {
        this.GetPadding = new Thickness(uniformPadding);
        return this;
    }

    public PdfElementData Padding(double horizontalPadding, double verticalPadding)
    {
        this.GetPadding = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    public PdfElementData Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        this.GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomMargin);
        return this;
    }

    public PdfElementData WidthRequest(double width)
    {
        this.GetWidthRequest = width;
        return this;
    }

    public PdfElementData HeightRequest(double height)
    {
        this.GetHeightRequest = height;
        return this;
    }

    public PdfElementData BackgroundColor(Color? color)
    {
        this.GetBackgroundColor = color;
        return this;
    }

    public PdfElementData HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        this.GetHorizontalOptions = layoutAlignment;
        _horizontalOptionsSet = true;
        return this;
    }

    public PdfElementData VerticalOptions(LayoutAlignment layoutAlignment)
    {
        this.GetVerticalOptions = layoutAlignment;
        _verticalOptionsSet = true;
        return this;
    }

    internal void ApplyContextualDefaults(LayoutAlignment horizontal, LayoutAlignment vertical)
    {
        if (!_horizontalOptionsSet)
        {
            this.GetHorizontalOptions = horizontal;
        }
        if (!_verticalOptionsSet)
        {
            this.GetVerticalOptions = vertical;
        }
    }

    internal void SetRow(int row)
    {
        if (row < 0) throw new ArgumentOutOfRangeException(nameof(row), "Row must be a non-negative integer.");
        this.GridRow = row;
    }

    internal void SetColumn(int column)
    {
        if (column < 0) throw new ArgumentOutOfRangeException(nameof(column), "Column must be a non-negative integer.");
        this.GridColumn = column;
    }

    internal void SetRowSpan(int span)
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "RowSpan must be a positive integer.");
        this.GridRowSpan = span;
    }

    internal void SetColumnSpan(int span)
    {
        if (span < 1) throw new ArgumentOutOfRangeException(nameof(span), "ColumnSpan must be a positive integer.");
        this.GridColumnSpan = span;
    }

    int IPdfGridCellInfo.Row => GridRow;
    int IPdfGridCellInfo.Column => GridColumn;
    int IPdfGridCellInfo.RowSpan => GridRowSpan;
    int IPdfGridCellInfo.ColumnSpan => GridColumnSpan;

    public PdfElementData Style(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Style key cannot be null or whitespace.", nameof(key));
        }
        this.StyleKey = key;
        return this;
    }
}
