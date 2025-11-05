namespace MauiPdfGenerator.Common.Models.Elements;

internal class PdfHorizontalLineData : PdfElementData
{
    public const float DefaultThickness = 1f;
    public static readonly Color DefaultColor = Colors.Black;

    internal float CurrentThickness { get; set; } = DefaultThickness;
    internal Color CurrentColor { get; set; } = DefaultColor;

    internal PdfHorizontalLineData()
    {
    }
}
