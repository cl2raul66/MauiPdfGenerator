using MauiPdfGenerator.Fluent.Enums;
using MauiPdfGenerator.Fluent.Interfaces.Elements;

namespace MauiPdfGenerator.Implementation.Builders;

/// <summary>
/// Internal implementation for building individual styled text spans.
/// </summary>
internal class SpanBuilder : IPdfSpanBuilder
{
    // Store configuration for this specific span
    public string? ConfiguredText { get; private set; }
    public Color? ConfiguredTextColor { get; private set; }
    public string? ConfiguredFontFamily { get; private set; }
    public double? ConfiguredFontSize { get; private set; } // Nullable: Inherits from Paragraph if null
    public PdfFontAttributes? ConfiguredFontAttributes { get; private set; } // Nullable: Inherits if null? Or combines? Let's assume override/combine later.
    public PdfTextDecorations? ConfiguredTextDecorations { get; private set; } // Nullable: Inherits if null? Or combines?
    public Color? ConfiguredBackgroundColor { get; private set; }
    public double? ConfiguredLineHeight { get; private set; } // Nullable: Inherits

    public IPdfSpanBuilder Text(string text)
    {
        ConfiguredText = text;
        return this;
    }

    public IPdfSpanBuilder TextColor(Color? color)
    {
        ConfiguredTextColor = color;
        return this;
    }

    public IPdfSpanBuilder FontFamily(string fontFamily)
    {
        ConfiguredFontFamily = fontFamily;
        return this;
    }

    public IPdfSpanBuilder FontSize(double size)
    {
        ConfiguredFontSize = size > 0 ? size : null;
        return this;
    }

    public IPdfSpanBuilder FontAttributes(PdfFontAttributes attributes)
    {
        ConfiguredFontAttributes = attributes;
        return this;
    }

    public IPdfSpanBuilder TextDecorations(PdfTextDecorations decorations)
    {
        ConfiguredTextDecorations = decorations;
        return this;
    }

    public IPdfSpanBuilder BackgroundColor(Color? color)
    {
        ConfiguredBackgroundColor = color;
        return this;
    }

    public IPdfSpanBuilder LineHeight(double multiplier)
    {
        ConfiguredLineHeight = multiplier > 0 ? multiplier : null;
        return this;
    }
}

// Fin namespace
