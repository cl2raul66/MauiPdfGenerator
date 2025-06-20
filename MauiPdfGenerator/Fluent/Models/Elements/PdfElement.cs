namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfElement
{
    internal Thickness GetMargin { get; private set; } = Thickness.Zero;
    internal Thickness GetPadding { get; private set; } = Thickness.Zero;
    internal double? GetWidthRequest { get; private set; }
    internal double? GetHeightRequest { get; private set; }

    internal int GridRow { get; private set; }
    internal int GridColumn { get; private set; }
    internal int GridRowSpan { get; private set; } = 1;
    internal int GridColumnSpan { get; private set; } = 1;
    internal bool IsGridPositionExplicit { get; private set; }

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

    internal PdfElement Margin(Thickness margin)
    {
        GetMargin = margin;
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

    public PdfElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomMargin);
        return this;
    }

    internal PdfElement Padding(Thickness padding)
    {
        GetPadding = padding;
        return this;
    }

    public PdfElement WidthRequest(double width)
    {
        GetWidthRequest = width > 0 ? width : null;
        return this;
    }

    public PdfElement HeightRequest(double height)
    {
        GetHeightRequest = height > 0 ? height : null;
        return this;
    }

    public PdfElement Row(int row)
    {
        GridRow = row > 0 ? row : 0;
        IsGridPositionExplicit = true;
        return this;
    }

    public PdfElement Column(int column)
    {
        GridColumn = column > 0 ? column : 0;
        IsGridPositionExplicit = true;
        return this;
    }

    public PdfElement RowSpan(int span)
    {
        GridRowSpan = span > 1 ? span : 1;
        return this;
    }

    public PdfElement ColumnSpan(int span)
    {
        GridColumnSpan = span > 1 ? span : 1;
        return this;
    }
}
