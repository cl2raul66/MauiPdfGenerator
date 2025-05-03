using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiPdfGenerator.Fluent.Models.Elements;

public class PdfHorizontalLine : PdfElement
{
    public const float DefaultThickness = 1f;
    public static readonly Color DefaultColor = Colors.Black;

    public float CurrentThickness { get; private set; } = DefaultThickness;

    public Color CurrentColor { get; private set; } = DefaultColor;

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
