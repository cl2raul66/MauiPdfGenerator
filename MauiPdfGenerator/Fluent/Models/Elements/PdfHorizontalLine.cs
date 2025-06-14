namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfHorizontalLine : PdfElement
{
    public const float DefaultThickness = 1f;
    public static readonly Color DefaultColor = Colors.Black;

    internal float CurrentThickness { get; private set; } = DefaultThickness;

    internal Color CurrentColor { get; private set; } = DefaultColor;

    public PdfHorizontalLine()
    {
    }

    public new PdfHorizontalLine Margin(double uniformMargin) { base.Margin(uniformMargin); return this; }
    public new PdfHorizontalLine Margin(double horizontalMargin, double verticalMargin) { base.Margin(horizontalMargin, verticalMargin); return this; }
    public new PdfHorizontalLine Margin(double leftMargin, double topMargin, double rightMargin, double bottomMargin) { base.Margin(leftMargin, topMargin, rightMargin, bottomMargin); return this; }
    public new PdfHorizontalLine Padding(double uniformPadding) { base.Padding(uniformPadding); return this; }
    public new PdfHorizontalLine Padding(double horizontalPadding, double verticalPadding) { base.Padding(horizontalPadding, verticalPadding); return this; }
    public new PdfHorizontalLine Padding(double leftPadding, double topPadding, double rightPadding, double bottomMargin) { base.Padding(leftPadding, topPadding, rightPadding, bottomMargin); return this; }
    public new PdfHorizontalLine WidthRequest(double width) { base.WidthRequest(width); return this; }
    public new PdfHorizontalLine HeightRequest(double height) { base.HeightRequest(height); return this; }


    public PdfHorizontalLine Thickness(float value)
    {
        CurrentThickness = value > 0 ? value : DefaultThickness;
        return this;
    }

    public PdfHorizontalLine Color(Color color)
    {
        CurrentColor = color ?? DefaultColor;
        return this;
    }
}
