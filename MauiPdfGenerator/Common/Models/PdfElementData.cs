namespace MauiPdfGenerator.Common.Models;

internal abstract class PdfElementData : IPdfGridCellInfo
{
    internal PdfElementData? Parent { get; set; }

    internal Thickness GetMargin { get; private set; }
    internal Thickness GetPadding { get; private set; }
    internal double? GetWidthRequest { get; private set; }
    internal double? GetHeightRequest { get; private set; }
    internal Color? GetBackgroundColor { get; private set; }
    internal LayoutAlignment GetHorizontalOptions { get; private set; } = LayoutAlignment.Fill;
    internal LayoutAlignment GetVerticalOptions { get; private set; } = LayoutAlignment.Fill;

    internal int GridRow { get; set; } = 0;
    internal int GridColumn { get; set; } = 0;
    internal int GridRowSpan { get; set; } = 1;
    internal int GridColumnSpan { get; set; } = 1;

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
        return this;
    }

    public PdfElementData VerticalOptions(LayoutAlignment layoutAlignment)
    {
        this.GetVerticalOptions = layoutAlignment;
        return this;
    }

    int IPdfGridCellInfo.Row => GridRow;
    int IPdfGridCellInfo.Column => GridColumn;
    int IPdfGridCellInfo.RowSpan => GridRowSpan;
    int IPdfGridCellInfo.ColumnSpan => GridColumnSpan;
}
