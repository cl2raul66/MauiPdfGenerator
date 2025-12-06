using MauiPdfGenerator.Common.Models.Styling;

namespace MauiPdfGenerator.Common.Models.Elements;

internal class PdfHorizontalLineData : PdfElementData
{
    public const float DefaultThickness = 1f;
    public static readonly Color DefaultColor = Colors.Black;

    internal PdfStyledProperty<float> ThicknessProp { get; } = new(DefaultThickness);
    internal PdfStyledProperty<Color> ColorProp { get; } = new(DefaultColor);

    internal float CurrentThickness => ThicknessProp.Value;
    internal Color CurrentColor => ColorProp.Value;

    internal PdfHorizontalLineData() : base() { }
}
