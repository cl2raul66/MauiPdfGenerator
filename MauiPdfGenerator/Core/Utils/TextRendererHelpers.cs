using System;
using System.Collections.Generic;
using MauiPdfGenerator.Core.Interfaces;
using MauiPdfGenerator.Core.Implementation;

namespace MauiPdfGenerator.Core.Utils;

/// <summary>
/// Interface for measuring text dimensions.
/// </summary>
public interface ITextMeasurer
{
    /// <summary>
    /// Measures the width of the given text.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The width of the text.</returns>
    float MeasureWidth(string text);
}

/// <summary>
/// Interface for measuring text dimensions including font metrics.
/// </summary>
public interface IFontMeasurer : ITextMeasurer
{
    /// <summary>
    /// Gets the ascent of the font.
    /// </summary>
    float Ascent { get; }

    /// <summary>
    /// Gets the descent of the font.
    /// </summary>
    float Descent { get; }

    /// <summary>
    /// Gets the line advance of the font.
    /// </summary>
    float LineAdvance { get; }
}

/// <summary>
/// Helper class for text rendering operations including wrapping and decorations.
/// </summary>
public static class TextRendererHelpers
{
    /// <summary>
    /// Aplica decoraciones comunes (e.g., subrayado) a fragmentos.
    /// </summary>
    public static void ApplyDecorations(IEnumerable<ITextFragment> fragments, DecorationOptions options)
    {
        foreach (var fragment in fragments)
        {
            // Lógica genérica de decoraciones...
        }
    }

    /// <summary>
    /// Measures the total height of text lines asynchronously.
    /// </summary>
    /// <param name="lines">The text lines to measure.</param>
    /// <param name="measurer">The font measurer to use for height calculations.</param>
    /// <returns>The total height of the lines.</returns>
    public static async Task<float> MeasureLinesAsync(IEnumerable<ITextLine> lines, IFontMeasurer measurer)
    {
        var lineList = lines.ToList();
        if (lineList.Count == 0) return 0;
        if (lineList.Count == 1)
        {
            return measurer.LineAdvance;
        }
        else
        {
            float visualTopOffset = -measurer.Ascent;
            float visualBottomOffset = measurer.Descent;
            return visualTopOffset + ((lineList.Count - 1) * measurer.LineAdvance) + visualBottomOffset;
        }
    }
}

public record DecorationOptions
{
    public bool Underline { get; init; }
    public bool Strikethrough { get; init; }
}