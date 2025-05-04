namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfElement
{
    internal Thickness GetMargin { get; private set; } = Thickness.Zero;

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

    public PdfElement Margin(Thickness margin)
    {
        GetMargin = margin;
        return this;
    }
}
