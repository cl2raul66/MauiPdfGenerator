namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfElement
{
    public Thickness Margin { get; protected set; } = Thickness.Zero;

    public PdfElement SetMargin(double uniformMargin)
    {
        Margin = new Thickness(uniformMargin);
        return this;
    }

    public PdfElement SetMargin(double horizontalMargin, double verticalMargin)
    {
        Margin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    public PdfElement SetMargin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        Margin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    public PdfElement SetMargin(Thickness margin)
    {
        Margin = margin;
        return this;
    }
}
