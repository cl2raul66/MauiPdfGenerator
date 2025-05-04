namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfHorizontalLine : PdfElement
{
    public const float DefaultThickness = 1f;
    public static readonly Color DefaultColor = Colors.Black;

    internal float CurrentThickness { get; private set; } = DefaultThickness;

    internal Color CurrentColor { get; private set; } = DefaultColor;

    public PdfHorizontalLine()
    {
        // Initialize with defaults
    }

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
