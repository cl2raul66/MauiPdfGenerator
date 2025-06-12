namespace MauiPdfGenerator.Fluent.Models.Elements;

public abstract class PdfElement
{
    internal Thickness GetMargin { get; private set; } = Thickness.Zero;
    internal Thickness GetPadding { get; private set; } = Thickness.Zero;

    /// <summary>
    /// Sets a uniform margin for the element.
    /// </summary>
    public PdfElement Margin(double uniformMargin)
    {
        GetMargin = new Thickness(uniformMargin);
        return this;
    }

    /// <summary>
    /// Sets the horizontal and vertical margins for the element.
    /// </summary>
    public PdfElement Margin(double horizontalMargin, double verticalMargin)
    {
        GetMargin = new Thickness(horizontalMargin, verticalMargin);
        return this;
    }

    /// <summary>
    /// Sets the margin for each side of the element.
    /// </summary>
    public PdfElement Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin)
    {
        GetMargin = new Thickness(leftMargin, topMargin, rightMargin, bottomMargin);
        return this;
    }

    /// <summary>
    /// Sets the margin for the element from a Thickness object.
    /// </summary>
    internal PdfElement Margin(Thickness margin)
    {
        GetMargin = margin;
        return this;
    }

    /// <summary>
    /// Sets a uniform padding for the element.
    /// </summary>
    public PdfElement Padding(double uniformPadding)
    {
        GetPadding = new Thickness(uniformPadding);
        return this;
    }

    /// <summary>
    /// Sets the horizontal and vertical padding for the element.
    /// </summary>
    public PdfElement Padding(double horizontalPadding, double verticalPadding)
    {
        GetPadding = new Thickness(horizontalPadding, verticalPadding);
        return this;
    }

    /// <summary>
    /// Sets the padding for each side of the element.
    /// </summary>
    public PdfElement Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin)
    {
        GetPadding = new Thickness(leftPadding, topPadding, rightPadding, bottomMargin);
        return this;
    }

    /// <summary>
    /// Sets the padding for the element from a Thickness object.
    /// </summary>
    internal PdfElement Padding(Thickness padding)
    {
        GetPadding = padding;
        return this;
    }
}
