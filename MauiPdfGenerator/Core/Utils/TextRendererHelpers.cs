using MauiPdfGenerator.Core.Interfaces;

namespace MauiPdfGenerator.Core.Utils;

internal interface ITextMeasurer
{
    float MeasureWidth(string text);
}

internal interface IFontMeasurer : ITextMeasurer
{
    float Ascent { get; }
    float Descent { get; }
    float LineAdvance { get; }
}

internal static class TextRendererHelpers
{
    public static async Task<float> MeasureLinesAsync(IEnumerable<ITextLine> lines, IFontMeasurer measurer)
    {
        if (!lines.Any()) return 0;
        if (lines.Count() == 1)
        {
            return measurer.LineAdvance;
        }
        else
        {
            float visualTopOffset = -measurer.Ascent;
            float visualBottomOffset = measurer.Descent;
            return visualTopOffset + ((lines.Count() - 1) * measurer.LineAdvance) + visualBottomOffset;
        }
    }
}

internal record DecorationOptions
{
    public bool Underline { get; init; }
    public bool Strikethrough { get; init; }
}
