namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfLayoutElement : PdfElement
{
    protected readonly List<PdfElement> _children = [];

    internal float GetSpacing { get; private set; }
    internal Color? GetBackgroundColor { get; private set; }
    internal LayoutAlignment GetHorizontalOptions { get; private protected set; }
    internal LayoutAlignment GetVerticalOptions { get; private protected set; }

    internal IReadOnlyList<PdfElement> GetChildren => _children.AsReadOnly();

    internal void Add(PdfElement element)
    {
        _children.Add(element);
    }

    public new PdfLayoutElement Margin(double uniformMargin)
    {
        base.Margin(uniformMargin);
        return this;
    }

    public new PdfLayoutElement Margin(double horizontalMargin, double verticalMargin)
    {
        base.Margin(horizontalMargin, verticalMargin);
        return this;
    }

    public new PdfLayoutElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        base.Margin(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public new PdfLayoutElement Padding(double uniformPadding)
    {
        base.Padding(uniformPadding);
        return this;
    }

    public new PdfLayoutElement Padding(double horizontalPadding, double verticalPadding)
    {
        base.Padding(horizontalPadding, verticalPadding);
        return this;
    }

    public new PdfLayoutElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        base.Padding(leftPadding, topPadding, rightPadding, bottomMargin);
        return this;
    }

    public new PdfLayoutElement WidthRequest(double width)
    {
        base.WidthRequest(width);
        return this;
    }

    public new PdfLayoutElement HeightRequest(double height)
    {
        base.HeightRequest(height);
        return this;
    }

    public PdfLayoutElement Spacing(float value)
    {
        GetSpacing = value >= 0 ? value : 0;
        return this;
    }

    public PdfLayoutElement BackgroundColor(Color? color)
    {
        GetBackgroundColor = color;
        return this;
    }

    public PdfLayoutElement HorizontalOptions(LayoutAlignment layoutAlignment)
    {
        GetHorizontalOptions = layoutAlignment;
        return this;
    }

    public PdfLayoutElement VerticalOptions(LayoutAlignment layoutAlignment)
    {
        GetVerticalOptions = layoutAlignment;
        return this;
    }
}
